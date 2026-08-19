using FUNewsManagement_AnalyticsAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace FUNewsManagement_AnalyticsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class RecommendController : ControllerBase
    {
        private readonly AnalyticsDbContext _context;

        public RecommendController(AnalyticsDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRecommendations(string id)
        {
            var sourceArticle = await _context.NewsArticles
                .Include(n => n.Tags)
                .FirstOrDefaultAsync(n => n.NewsArticleId == id);

            if (sourceArticle == null)
            {
                return NotFound();
            }

            var sourceTagIds = sourceArticle.Tags.Select(t => t.TagId).ToList();

            // Fetch active articles except the source one
            var otherArticles = await _context.NewsArticles
                .Include(n => n.Tags)
                .Where(n => n.NewsArticleId != id && n.NewsStatus == true)
                .ToListAsync();

            // Calculate similarity score: 
            // 2 points for matching category
            // 1 point for each shared tag
            var recommendations = otherArticles
                .Select(a => new
                {
                    Article = a,
                    Score = (a.CategoryId == sourceArticle.CategoryId ? 2 : 0) + a.Tags.Count(t => sourceTagIds.Contains(t.TagId))
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Article.CreatedDate)
                .Take(3)
                .Select(x => new
                {
                    x.Article.NewsArticleId,
                    x.Article.NewsTitle,
                    Headline = x.Article.Headline,
                    x.Article.ImageUrl,
                    x.Article.CreatedDate,
                    x.Article.ViewCount
                })
                .ToList();

            return Ok(recommendations);
        }
    }
}
