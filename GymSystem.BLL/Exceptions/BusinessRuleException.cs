namespace GymSystem.BLL.Exceptions;

// بنرميها لما قاعدة شغل تتكسر، والرسالة بتتعرض للمستخدم
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message)
    {
    }
}