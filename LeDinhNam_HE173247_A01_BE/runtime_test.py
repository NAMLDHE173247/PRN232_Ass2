import requests
import json
import urllib3
urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

CORE_URL = "https://localhost:7001/api"
ANALYTICS_URL = "https://localhost:7002/api"

print("--- 3. JWT Runtime Test ---")
# 1. Login Admin
admin_resp = requests.post(f"{CORE_URL}/auth/login", json={"email": "admin@FUNewsManagementSystem.org", "password": "@@abc123@@"}, verify=False)
if admin_resp.status_code == 200:
    admin_token = admin_resp.json()["accessToken"]
    print("Admin login: 200")
else:
    print(f"Admin login failed: {admin_resp.status_code}")
    admin_token = ""

# 2. Login Staff
staff_resp = requests.post(f"{CORE_URL}/auth/login", json={"email": "staff.hasnews@test.com", "password": "234@234a"}, verify=False)
if staff_resp.status_code == 200:
    staff_token = staff_resp.json()["accessToken"]
    print("Staff login: 200")
else:
    print(f"Staff login failed: {staff_resp.status_code}")
    staff_token = ""

# Test Dashboard with Admin Token
dash_admin_resp = requests.get(f"{ANALYTICS_URL}/analytics/dashboard", headers={"Authorization": f"Bearer {admin_token}"}, verify=False)
print(f"Admin token -> Dashboard: {dash_admin_resp.status_code}")

# Test Dashboard with Staff Token
dash_staff_resp = requests.get(f"{ANALYTICS_URL}/analytics/dashboard", headers={"Authorization": f"Bearer {staff_token}"}, verify=False)
print(f"Staff token -> Dashboard: {dash_staff_resp.status_code}")

# Test Dashboard with No Token
dash_no_resp = requests.get(f"{ANALYTICS_URL}/analytics/dashboard", verify=False)
print(f"No token -> Dashboard: {dash_no_resp.status_code}")

print("\n--- 4. Dashboard Runtime Test ---")
if dash_admin_resp.status_code == 200:
    print(f"No filter: {dash_admin_resp.json()}")

filter_resp = requests.get(f"{ANALYTICS_URL}/analytics/dashboard?status=true", headers={"Authorization": f"Bearer {admin_token}"}, verify=False)
print(f"Status=true filter: {filter_resp.json()}")

bad_filter_resp = requests.get(f"{ANALYTICS_URL}/analytics/dashboard?startDate=2026-01-01&endDate=2025-01-01", headers={"Authorization": f"Bearer {admin_token}"}, verify=False)
print(f"startDate > endDate: {bad_filter_resp.status_code}")

print("\n--- 5. Trending Runtime Test ---")
trending_resp = requests.get(f"{ANALYTICS_URL}/analytics/trending", headers={"Authorization": f"Bearer {admin_token}"}, verify=False)
if trending_resp.status_code == 200:
    trending_data = trending_resp.json()
    print("Trending data order:")
    for item in trending_data:
        print(f"  {item['newsArticleId']}: {item['viewCount']} views")
else:
    print(f"Trending error: {trending_resp.status_code}")

print("\n--- 6. Recommendation Scoring Test ---")
# Call recommendation with no JWT
# Pick a source article id. Let's find one from trending.
if trending_resp.status_code == 200 and len(trending_data) > 0:
    source_id = trending_data[0]['newsArticleId']
    rec_resp = requests.get(f"{ANALYTICS_URL}/recommend/{source_id}", verify=False)
    print(f"Recommend (No JWT) for {source_id}: {rec_resp.status_code}")
    if rec_resp.status_code == 200:
        rec_data = rec_resp.json()
        print(f"Recommendation count: {len(rec_data)}")
        for rec in rec_data:
            print(f"  {rec['newsArticleId']} ({rec['newsTitle']})")
            if rec['newsArticleId'] == source_id:
                print("  ERROR: Source article appeared in recommendations!")
else:
    print("No trending data to pick source_id from.")

rec_404_resp = requests.get(f"{ANALYTICS_URL}/recommend/invalid_id_123", verify=False)
print(f"Recommend invalid ID: {rec_404_resp.status_code}")

print("\n--- 7. Excel Runtime Test ---")
excel_resp = requests.get(f"{ANALYTICS_URL}/analytics/export", headers={"Authorization": f"Bearer {admin_token}"}, verify=False)
print(f"Export status: {excel_resp.status_code}")
print(f"Content type: {excel_resp.headers.get('Content-Type')}")
if excel_resp.status_code == 200:
    with open("test_export.xlsx", "wb") as f:
        f.write(excel_resp.content)
    print(f"Saved {len(excel_resp.content)} bytes to test_export.xlsx")
