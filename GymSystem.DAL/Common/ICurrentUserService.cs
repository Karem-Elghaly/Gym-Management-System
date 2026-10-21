using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymSystem.DAL.Common;

// عقد بيقول "أنا أقدر أجيبلك اسم المستخدم الحالي"
// الـ DAL بتعتمد على العقد ده بس، والـ PL هو اللي هينفذه بالـ HttpContext
// كده الـ DAL فاضلة مستقلة ومتعرفش حاجة عن الـ Web
public interface ICurrentUserService
{
    // اسم المستخدم الحالي، أو null لو مفيش حد عامل Login (زي وقت الـ Seeder)
    string? UserName { get; }
}
