import requests
import json
import urllib3
urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

CORE_URL = "https://localhost:7001/api"
AI_URL = "https://localhost:7003/api"

print("--- JWT Setup ---")
admin_resp = requests.post(f"{CORE_URL}/auth/login", json={"email": "admin@FUNewsManagementSystem.org", "password": "@@abc123@@"}, verify=False)
if admin_resp.status_code == 200:
    admin_token = admin_resp.json()["accessToken"]
    print("Admin login: 200")
else:
    print(f"Admin login failed: {admin_resp.status_code}")
    admin_token = ""

staff_resp = requests.post(f"{CORE_URL}/auth/login", json={"email": "staff.hasnews@test.com", "password": "234@234a"}, verify=False)
if staff_resp.status_code == 200:
    staff_token = staff_resp.json()["accessToken"]
    print("Staff login: 200")
else:
    print(f"Staff login failed: {staff_resp.status_code}")
    staff_token = ""

print("\n--- Test A: JWT Authorization ---")
suggest_staff_resp = requests.post(f"{AI_URL}/ai/suggest-tags", json={"content": "test"}, headers={"Authorization": f"Bearer {staff_token}"}, verify=False)
print(f"Staff token -> Suggest: {suggest_staff_resp.status_code}")

suggest_no_resp = requests.post(f"{AI_URL}/ai/suggest-tags", json={"content": "test"}, verify=False)
print(f"No token -> Suggest: {suggest_no_resp.status_code}")

print("\n--- Test B: Empty content ---")
empty_resp = requests.post(f"{AI_URL}/ai/suggest-tags", json={"content": ""}, headers={"Authorization": f"Bearer {staff_token}"}, verify=False)
print(f"Empty content -> Suggest: {empty_resp.status_code}")

print("\n--- Test C & D: Real suggestion & Stop words ---")
stop_words_resp = requests.post(f"{AI_URL}/ai/suggest-tags", json={"content": "the and is of to in"}, headers={"Authorization": f"Bearer {staff_token}"}, verify=False)
print(f"Stop words content -> Suggest: {stop_words_resp.json()}")

content_real = "Machine learning and artificial intelligence are revolutionizing technology and education."
real_resp = requests.post(f"{AI_URL}/ai/suggest-tags", json={"content": content_real}, headers={"Authorization": f"Bearer {staff_token}"}, verify=False)
if real_resp.status_code == 200:
    print("Real suggestion initial order:")
    tags = real_resp.json().get('tags', [])
    for t in tags:
        print(f"  {t['name']}: {t['confidence']:.4f}")
else:
    print(f"Real suggestion failed: {real_resp.status_code}")

print("\n--- Test E: Learning Cache ---")
# Learn nonexistent tag
bad_learn_resp = requests.post(f"{AI_URL}/ai/learn", json={"keyword": "machine", "tagName": "NonExistentTagXYZ"}, headers={"Authorization": f"Bearer {staff_token}"}, verify=False)
print(f"Learn nonexistent tag: {bad_learn_resp.status_code}")

# Repeatedly learn a tag that initially scored lower.
# Let's see the initial tags. Assume 'Technology' and 'Machine Learning' are there.
target_tag = "Technology" # Usually single word matches frequency 1, while phrase matches 5. So 'Technology' will be lower than 'Machine Learning'.
print(f"Learning tag '{target_tag}' for keyword 'technology' 20 times...")
for _ in range(20):
    requests.post(f"{AI_URL}/ai/learn", json={"keyword": "technology", "tagName": target_tag}, headers={"Authorization": f"Bearer {staff_token}"}, verify=False)

# Get suggestions again
real_resp_after = requests.post(f"{AI_URL}/ai/suggest-tags", json={"content": content_real}, headers={"Authorization": f"Bearer {staff_token}"}, verify=False)
if real_resp_after.status_code == 200:
    print("Real suggestion order after learning:")
    tags_after = real_resp_after.json().get('tags', [])
    for t in tags_after:
        print(f"  {t['name']}: {t['confidence']:.4f}")
else:
    print(f"Real suggestion after failed: {real_resp_after.status_code}")
