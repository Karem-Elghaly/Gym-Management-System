using AutoMapper;
using GymSystem.BLL.ViewModels.Attendance;
using GymSystem.BLL.ViewModels.Classes;
using GymSystem.BLL.ViewModels.Members;
using GymSystem.BLL.ViewModels.Payments;
using GymSystem.BLL.ViewModels.Plans;
using GymSystem.BLL.ViewModels.Subscriptions;
using GymSystem.BLL.ViewModels.Trainers;
// using صريح عشان نستخدم BookingStatus بالاسم المباشر جوه الـ Expression
using GymSystem.DAL.Common.Enums;
using GymSystem.DAL.Entities;

namespace GymSystem.BLL.Mapping;

// Profile هو الكلاس اللي AutoMapper بيقرا منه قواعد التحويل
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ===================== Members =====================

        CreateMap<Member, MemberListItemViewModel>()
            .ForMember(d => d.CurrentPlanName, o => o.Ignore())
            .ForMember(d => d.HasActiveSubscription, o => o.Ignore());

        CreateMap<Member, MemberDetailsViewModel>()
            .ForMember(d => d.Age, o => o.Ignore());

        CreateMap<Member, MemberViewModel>()
            .ForMember(d => d.ExistingImageName, o => o.MapFrom(s => s.ImageName))
            .ForMember(d => d.ImageFile, o => o.Ignore());

        CreateMap<MemberViewModel, Member>()
            .ForMember(d => d.DateOfBirth, o => o.MapFrom(s => s.DateOfBirth.GetValueOrDefault()))
            .ForMember(d => d.Gender, o => o.MapFrom(s => s.Gender.GetValueOrDefault()))
            .ForMember(d => d.ImageName, o => o.Ignore())
            .ForMember(d => d.Id, o => o.Ignore());

        // ===================== Plans =====================

        CreateMap<MembershipPlan, PlanViewModel>()
            .ReverseMap()
            .ForMember(d => d.Id, o => o.Ignore());

        CreateMap<MembershipPlan, PlanListItemViewModel>();

        // ===================== Trainers =====================

        CreateMap<Trainer, TrainerViewModel>()
            .ReverseMap()
            .ForMember(d => d.Id, o => o.Ignore());

        // الحصص بتتقارن بالتوقيت المحلي (DateTime.Now) لأن الموظف بيدخلها بتوقيت الجيم
        CreateMap<Trainer, TrainerListItemViewModel>()
            .ForMember(d => d.UpcomingClassesCount,
                o => o.MapFrom(s => s.Classes.Count(c => c.StartTime > DateTime.Now)));

        // ===================== Subscriptions =====================

        CreateMap<Subscription, SubscriptionListItemViewModel>()
            .ForMember(d => d.MemberName, o => o.MapFrom(s => s.Member.FullName))
            .ForMember(d => d.PlanName, o => o.MapFrom(s => s.Plan.Name))
            .ForMember(d => d.TotalPaid, o => o.Ignore());

        // ===================== Payments =====================

        CreateMap<Payment, PaymentListItemViewModel>()
            .ForMember(d => d.MemberName, o => o.MapFrom(s => s.Subscription.Member.FullName))
            .ForMember(d => d.PlanName, o => o.MapFrom(s => s.Subscription.Plan.Name));

        // ===================== Classes =====================

        CreateMap<GymClass, ClassViewModel>()
            .ReverseMap()
            .ForMember(d => d.Id, o => o.Ignore());

        CreateMap<GymClass, ClassListItemViewModel>()
            .ForMember(d => d.TrainerName, o => o.MapFrom(s => s.Trainer.FullName))
            .ForMember(d => d.BookedCount,
                o => o.MapFrom(s => s.Bookings.Count(b => b.Status != BookingStatus.Cancelled)));

        CreateMap<GymClass, ClassDetailsViewModel>()
            .ForMember(d => d.TrainerName, o => o.MapFrom(s => s.Trainer.FullName));

        CreateMap<Booking, BookedMemberViewModel>()
            .ForMember(d => d.BookingId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.MemberName, o => o.MapFrom(s => s.Member.FullName));

        // ===================== Attendance =====================

        CreateMap<Attendance, AttendanceListItemViewModel>()
            .ForMember(d => d.MemberName, o => o.MapFrom(s => s.Member.FullName));
    }
}