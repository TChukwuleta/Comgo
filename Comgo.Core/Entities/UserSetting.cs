namespace Comgo.Core.Entities
{
    public class UserSetting : GeneralEntity
    {
        public int? SecurityQuestionId { get; set; }
        public string SecurityQuestionResponse { get; set; }
        public int LocktimeStartHour { get; set; }
        public int LockTimeEndHour { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
    }
}
