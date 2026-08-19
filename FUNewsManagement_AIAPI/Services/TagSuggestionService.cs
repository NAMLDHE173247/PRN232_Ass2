using FUNewsManagement_AIAPI.Data;
using FUNewsManagement_AIAPI.DTOs;
using FUNewsManagement_AIAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace FUNewsManagement_AIAPI.Services;

public class TagSuggestionService : ITagSuggestionService
{
    private readonly AiDbContext _context;
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "a", "an", "the", "and", "or", "but", "is", "are", "was", "were", "be", "been",
        "to", "of", "in", "on", "for", "with", "at", "by", "from", "as", "it", "this", "that",
        "và", "là", "của", "có", "trong", "cho", "với", "một", "các", "được", "từ", "về"
    };

    public TagSuggestionService(AiDbContext context)
    {
        _context = context;
    }

    private string Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        var noPunc = Regex.Replace(text, @"[^\w\s]", " ");
        return Regex.Replace(noPunc, @"\s+", " ").Trim().ToLowerInvariant();
    }

    public async Task<SuggestTagsResponse> SuggestTagsAsync(SuggestTagsRequest request)
    {
        var normalizedContent = Normalize(request.Content);
        if (string.IsNullOrWhiteSpace(normalizedContent))
        {
            return new SuggestTagsResponse();
        }

        var tokens = normalizedContent.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                      .Where(t => !StopWords.Contains(t))
                                      .ToList();

        // 1. Calculate word frequency
        var wordFreq = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        foreach (var t in tokens)
        {
            if (!wordFreq.ContainsKey(t)) wordFreq[t] = 0;
            wordFreq[t] += 1.0;
        }

        // 2. Fetch existing tags
        var existingTags = await _context.Tags.AsNoTracking().ToListAsync();
        var tagScores = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        // 3. Base Score Calculation
        foreach (var tag in existingTags)
        {
            if (string.IsNullOrWhiteSpace(tag.TagName)) continue;
            
            var normalizedTag = Normalize(tag.TagName);
            if (string.IsNullOrWhiteSpace(normalizedTag)) continue;

            double baseScore = 0;

            // Multi-word phrase matching
            if (normalizedContent.Contains(normalizedTag))
            {
                baseScore += 5.0; // High base score for exact phrase match
            }

            // Individual word frequency matching
            var tagTokens = normalizedTag.Split(' ');
            foreach (var tt in tagTokens)
            {
                if (wordFreq.TryGetValue(tt, out var freq))
                {
                    baseScore += freq;
                }
            }

            if (baseScore > 0)
            {
                tagScores[tag.TagName] = baseScore;
            }
        }

        // 4. Learning Cache Boost
        // Get all unique tokens from content to match against Keyword in cache
        var distinctTokens = tokens.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        
        var learningCaches = await _context.TagLearningCaches
            .AsNoTracking()
            .Where(c => distinctTokens.Contains(c.Keyword))
            .ToListAsync();

        foreach (var cache in learningCaches)
        {
            // If the content contains a keyword that the user previously associated with this TagName
            // we boost the score of that TagName
            if (!tagScores.ContainsKey(cache.TagName))
            {
                // Optionally bring it in even if baseScore was 0, to allow learning of loosely related tags
                tagScores[cache.TagName] = 0.5; 
            }

            // Learning boost: log(1 + SelectedCount)
            double learningBoost = Math.Log10(1 + cache.SelectedCount) * 2.0; 
            tagScores[cache.TagName] += learningBoost;
        }

        // 5. Final Sort & Normalize Confidence
        if (!tagScores.Any())
        {
            return new SuggestTagsResponse();
        }

        var maxScore = tagScores.Values.Max();
        if (maxScore <= 0) maxScore = 1;

        var suggestions = tagScores
            .OrderByDescending(kv => kv.Value)
            .Take(5)
            .Select(kv => new SuggestedTagDto
            {
                Name = kv.Key,
                Confidence = Math.Clamp(kv.Value / maxScore, 0.01, 1.0)
            })
            .ToList();

        return new SuggestTagsResponse { Tags = suggestions };
    }

    public async Task<bool> LearnTagAsync(LearnTagRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Keyword) || string.IsNullOrWhiteSpace(request.TagName))
        {
            return false;
        }

        var normalizedKeyword = Normalize(request.Keyword);
        var normalizedTag = request.TagName.Trim();

        if (string.IsNullOrWhiteSpace(normalizedKeyword)) return false;

        // 1. Validate Tag exists in DB
        var tagExists = await _context.Tags.AnyAsync(t => t.TagName.ToLower() == normalizedTag.ToLower());
        if (!tagExists)
        {
            return false; // Reject nonexistent tag
        }

        // Fetch actual casing from DB
        var actualTag = await _context.Tags.FirstOrDefaultAsync(t => t.TagName.ToLower() == normalizedTag.ToLower());
        if (actualTag != null)
        {
            normalizedTag = actualTag.TagName;
        }

        // 2. Update or Insert Learning Cache
        var existingCache = await _context.TagLearningCaches
            .FirstOrDefaultAsync(c => c.Keyword.ToLower() == normalizedKeyword && c.TagName.ToLower() == normalizedTag.ToLower());

        if (existingCache != null)
        {
            await _context.TagLearningCaches
                .Where(c => c.Id == existingCache.Id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(b => b.SelectedCount, b => b.SelectedCount + 1)
                    .SetProperty(b => b.LastUpdated, DateTime.Now));
        }
        else
        {
            var newCache = new TagLearningCache
            {
                Keyword = normalizedKeyword,
                TagName = normalizedTag,
                SelectedCount = 1,
                LastUpdated = DateTime.Now
            };
            _context.TagLearningCaches.Add(newCache);
            await _context.SaveChangesAsync();
        }

        return true;
    }
}
