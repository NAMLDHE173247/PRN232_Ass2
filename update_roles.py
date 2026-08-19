import os
import re

controllers_dir = r'c:\Users\MSI\Desktop\PRN232\PRN232_Block5\ASS02\LeDinhNam_HE173247_A02\LeDinhNam_HE173247_A02_FE\ass01_FE\Presentation\Controllers'
admin_controllers = ['ReportController.cs']
staff_controllers = ['CategoryController.cs', 'TagController.cs', 'StaffNewsController.cs', 'MyHistoryController.cs', 'ProfileController.cs']

for f in os.listdir(controllers_dir):
    if f not in admin_controllers and f not in staff_controllers: continue
    
    path = os.path.join(controllers_dir, f)
    with open(path, 'r', encoding='utf-8') as file:
        content = file.read()
        
    if 'using ass01_FE.Infrastructure.Filters;' not in content:
        content = content.replace('namespace ass01_FE.Presentation.Controllers;', 'using ass01_FE.Infrastructure.Filters;\n\nnamespace ass01_FE.Presentation.Controllers;')
        
    role = '"Admin"' if f in admin_controllers else '"Staff"'
    
    if '[RoleAuthorize(' not in content:
        content = re.sub(r'(public class ' + f.replace('.cs', '') + r' : Controller)', f'[RoleAuthorize({role})]\n\\1', content)
        
        with open(path, 'w', encoding='utf-8') as file:
            file.write(content)
        print(f'Updated {f}')
