using GymSystem.DAL.Common.Enums;
using GymSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymSystem.DAL.Data;

// بيزرع بيانات تجريبية لتجربة المشروع. بيشتغل في بيئة التطوير بس (شوف WebApplicationExtensions)
public static class DevDataSeeder
{
    public static async Task SeedAsync(GymDbContext db)
    {
        // لو الداتا اتزرعت قبل كده ما نكررهاش (IgnoreQueryFilters عشان نشوف المحذوفين كمان)
        if (await db.Members.IgnoreQueryFilters().AnyAsync(m => m.Phone == "01010000001"))
            return;

        // الباقات الافتراضية بيزرعها DbSeeder، فلو مش موجودة نوقف
        var monthly = await db.MembershipPlans.FirstOrDefaultAsync(p => p.Name == "Monthly");
        var quarterly = await db.MembershipPlans.FirstOrDefaultAsync(p => p.Name == "Quarterly");
        var yearly = await db.MembershipPlans.FirstOrDefaultAsync(p => p.Name == "Yearly");
        if (monthly is null || quarterly is null || yearly is null) return;

        var now = DateTime.UtcNow;                         // للاشتراكات والمدفوعات والحضور
        var today = DateOnly.FromDateTime(now);
        var tomorrow = DateTime.Now.Date.AddDays(1);       // الحصص بتوقيت الجيم المحلي

        // ===== دوال مساعدة صغيرة عشان الكود ما يتكررش =====
        Member NewMember(string name, string phone, string? email, string dob, Gender gender, string address) => new()
        {
            FullName = name,
            Phone = phone,
            Email = email,
            DateOfBirth = DateOnly.Parse(dob),
            Gender = gender,
            Address = address
        };

        // startOffset و endOffset: عدد الأيام بالنسبة لليوم (سالب = في الماضي)
        Subscription NewSub(Member m, MembershipPlan p, int startOffset, int endOffset,
            SubscriptionStatus status, int? frozenOffset = null) => new()
            {
                Member = m,
                Plan = p,
                StartDate = today.AddDays(startOffset),
                EndDate = today.AddDays(endOffset),
                Status = status,
                PriceAtPurchase = p.Price,
                FrozenAt = frozenOffset is null ? null : today.AddDays(frozenOffset.Value)
            };

        void Pay(Subscription s, decimal amount, PaymentMethod method, int daysAgo, string note)
            => s.Payments.Add(new Payment
            {
                Amount = amount,
                Method = method,
                PaidAt = now.AddDays(-daysAgo),
                Notes = note
            });

        // ===== 1) الأعضاء =====
        var ahmed = NewMember("Ahmed Hassan", "01010000001", "ahmed.hassan@test.com", "1995-03-14", Gender.Male, "Mansoura, Dakahlia");
        var mohamed = NewMember("Mohamed Ali", "01010000002", "mohamed.ali@test.com", "1998-07-02", Gender.Male, "Mansoura, Dakahlia");
        var sara = NewMember("Sara Mahmoud", "01110000003", "sara.mahmoud@test.com", "2000-11-21", Gender.Female, "Talkha, Dakahlia");
        var omar = NewMember("Omar Khaled", "01210000004", null, "1992-01-30", Gender.Male, "Cairo");
        var nour = NewMember("Nour Ibrahim", "01510000005", "nour.ibrahim@test.com", "1999-05-09", Gender.Female, "Mansoura, Dakahlia");
        var youssef = NewMember("Youssef Adel", "01010000006", null, "2001-09-18", Gender.Male, "Damietta");
        var mona = NewMember("Mona Samir", "01110000007", "mona.samir@test.com", "1996-12-25", Gender.Female, "Mansoura, Dakahlia");
        var laila = NewMember("Laila Tarek", "01010000009", "laila.tarek@test.com", "1997-08-15", Gender.Female, "Mansoura, Dakahlia");

        // عضو محذوف Soft: لازم ما يظهرش في أي شاشة
        var hany = NewMember("Hany Fathy", "01210000008", null, "1990-04-04", Gender.Male, "Mansoura, Dakahlia");
        hany.IsDeleted = true;
        hany.DeletedAt = now;

        // ===== 2) الاشتراكات + المدفوعات =====
        // Ahmed: نشط ومدفوع بالكامل
        var subAhmed = NewSub(ahmed, monthly, -10, 20, SubscriptionStatus.Active);
        Pay(subAhmed, 500, PaymentMethod.Cash, 10, "Initial payment");

        // Mohamed: نشط، بيخلص بعد 3 أيام، مدفوع 300 من 500 (المتبقي 200)
        var subMohamed = NewSub(mohamed, monthly, -27, 3, SubscriptionStatus.Active);
        Pay(subMohamed, 300, PaymentMethod.Cash, 27, "Initial payment");

        // Sara: ربع سنوي نشط، بيخلص بعد 6 أيام، مدفوع بالكامل
        var subSara = NewSub(sara, quarterly, -84, 6, SubscriptionStatus.Active);
        Pay(subSara, 1350, PaymentMethod.Card, 84, "Initial payment");

        // Omar: تاريخه عدّى لكن Status لسه Active، فالـ Background Job هيحوله Expired عند التشغيل
        var subOmar = NewSub(omar, monthly, -60, -30, SubscriptionStatus.Active);
        Pay(subOmar, 500, PaymentMethod.Cash, 60, "Initial payment");

        // Nour: متجمد من 5 أيام
        var subNour = NewSub(nour, yearly, -100, 265, SubscriptionStatus.Frozen, frozenOffset: -5);
        Pay(subNour, 2000, PaymentMethod.Transfer, 100, "Bank transfer");

        // Youssef: ملغي
        var subYoussef = NewSub(youssef, monthly, -15, 15, SubscriptionStatus.Cancelled);

        // Laila: سنوي نشط، مدفوع 1000 من 4800 على دفعتين (المتبقي 3800)
        var subLaila = NewSub(laila, yearly, -5, 360, SubscriptionStatus.Active);
        Pay(subLaila, 700, PaymentMethod.Card, 5, "Initial payment");
        Pay(subLaila, 300, PaymentMethod.Cash, 1, "Second payment");

        // ===== 3) المدربين =====
        var mostafa = new Trainer { FullName = "Mostafa Kamal", Specialty = "Bodybuilding", Phone = "01011111111", Bio = "Certified strength coach, 8 years of experience." };
        var heba = new Trainer { FullName = "Heba Sayed", Specialty = "Yoga", Phone = "01111111112", Bio = "Yoga and mobility instructor." };
        var ali = new Trainer { FullName = "Ali Nabil", Specialty = "Cardio", Phone = "01211111113", Bio = "HIIT and cardio specialist." };

        // ===== 4) الحصص =====
        // سعة 2 وهتتملي بحجزين => حصة كاملة
        var yoga = new GymClass
        {
            Name = "Morning Yoga",
            Description = "Relaxing yoga flow",
            Trainer = heba,
            StartTime = tomorrow.AddHours(9),
            EndTime = tomorrow.AddHours(10),
            MaxCapacity = 2
        };
        var cardio = new GymClass
        {
            Name = "Cardio Blast",
            Description = "High intensity cardio",
            Trainer = ali,
            StartTime = tomorrow.AddHours(18),
            EndTime = tomorrow.AddHours(19),
            MaxCapacity = 20
        };
        var strength = new GymClass
        {
            Name = "Strength Training",
            Description = "Compound lifts",
            Trainer = mostafa,
            StartTime = tomorrow.AddDays(1).AddHours(17),
            EndTime = tomorrow.AddDays(1).AddHours(18.5),
            MaxCapacity = 15
        };
        // حصة فاتت (للتاريخ)
        var spinning = new GymClass
        {
            Name = "Evening Spinning",
            Description = "Past class for history",
            Trainer = ali,
            StartTime = DateTime.Now.Date.AddDays(-1).AddHours(10),
            EndTime = DateTime.Now.Date.AddDays(-1).AddHours(11),
            MaxCapacity = 10
        };

        // ===== 5) الحجوزات =====
        var bookings = new List<Booking>
        {
            new() { Member = ahmed, GymClass = yoga,     Status = BookingStatus.Booked },     // Morning Yoga: 2/2 كاملة
            new() { Member = sara,  GymClass = yoga,     Status = BookingStatus.Booked },
            new() { Member = ahmed, GymClass = cardio,   Status = BookingStatus.Booked },     // Cardio: 2/20
            new() { Member = laila, GymClass = cardio,   Status = BookingStatus.Booked },
            new() { Member = sara,  GymClass = strength, Status = BookingStatus.Cancelled },  // جرّب تحجز Sara تاني: لازم يرجّع نفس الحجز
            new() { Member = ahmed, GymClass = spinning, Status = BookingStatus.Attended, BookedAt = now.AddDays(-2) }
        };

        // ===== 6) الحضور =====
        var attendances = new List<Attendance>
        {
            new() { Member = ahmed, CheckInTime = now.AddMinutes(-60) },                                           // لسه جوه (جرّب Check out)
            new() { Member = sara,  CheckInTime = now.AddMinutes(-180), CheckOutTime = now.AddMinutes(-90) },      // دخل وخرج
            new() { Member = laila, CheckInTime = now.AddDays(-1),      CheckOutTime = now.AddDays(-1).AddHours(1) } // امبارح
        };

        // EF بيكتشف الكائنات المرتبطة ويضيفها لوحده، لكن بنضيف الأساسيين صراحةً للوضوح
        db.Members.AddRange(ahmed, mohamed, sara, omar, nour, youssef, mona, laila, hany);
        db.Subscriptions.AddRange(subAhmed, subMohamed, subSara, subOmar, subNour, subYoussef, subLaila);
        db.Trainers.AddRange(mostafa, heba, ali);
        db.GymClasses.AddRange(yoga, cardio, strength, spinning);
        db.Bookings.AddRange(bookings);
        db.Attendances.AddRange(attendances);

        // Save واحدة: يا كله يتحفظ يا ولا حاجة
        await db.SaveChangesAsync();
    }
}