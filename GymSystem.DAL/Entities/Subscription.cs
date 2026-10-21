using GymSystem.DAL.Common;
using GymSystem.DAL.Common.Enums;

namespace GymSystem.DAL.Entities;


public class Subscription : BaseEntity
{

    public int MemberId { get; set; }


    public int PlanId { get; set; }

    
    public Member Member { get; set; } = null!;


    public MembershipPlan Plan { get; set; } = null!;

  
    public DateOnly StartDate { get; set; }

    //  Service: StartDate + Plan.DurationInDays
    public DateOnly EndDate { get; set; }

    //  (Active / Expired / Frozen / Cancelled)

    // تاريخ بداية التجميد. null لو الاشتراك مش متجمد
    // لما يتفك التجميد: عدد الأيام (النهاردة - FrozenAt) بنضيفها على EndDate
    public DateOnly? FrozenAt { get; set; }

    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

  
    public decimal PriceAtPurchase { get; set; }

  
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}