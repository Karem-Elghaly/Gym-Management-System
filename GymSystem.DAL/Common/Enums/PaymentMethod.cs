namespace GymSystem.DAL.Common.Enums;

// طرق الدفع المتاحة في الجيم
public enum PaymentMethod
{
    // دفع كاش في الاستقبال
    Cash = 1,

    // دفع بكارت (فيزا أو ماستر كارد)
    Card = 2,

    // تحويل بنكي أو محفظة إلكترونية
    Transfer = 3
}