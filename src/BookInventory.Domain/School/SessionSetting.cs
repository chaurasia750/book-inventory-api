using BookInventory.Domain.Core;

namespace BookInventory.Domain.School;

/// <summary>
/// R19 (BRD Q-05): a company's two session cut-offs, one row per company, created on the first change; until then the BRD
/// defaults (1 February, 15 November) apply. Changing them changes only sessions computed afterwards.
/// </summary>
public class SessionSetting : CompanyOwnedRoot
{
    private SessionSetting()
    {
    }

    public int AprMarCutOffMonth { get; private set; }

    public int AprMarCutOffDay { get; private set; }

    public int JanDecCutOffMonth { get; private set; }

    public int JanDecCutOffDay { get; private set; }

    public SessionCutOff AprMar => new(AprMarCutOffMonth, AprMarCutOffDay);

    public SessionCutOff JanDec => new(JanDecCutOffMonth, JanDecCutOffDay);

    public static SessionSetting WithDefaults()
    {
        var setting = new SessionSetting();
        setting.Change(SessionCutOff.AprMarDefault, SessionCutOff.JanDecDefault);
        return setting;
    }

    /// <summary>R19: the cut-offs, each checked for its session type.</summary>
    public void Change(SessionCutOff aprMar, SessionCutOff janDec)
    {
        var checkedAprMar = SessionCutOff.Require(SessionType.AprMar, aprMar.Month, aprMar.Day);
        var checkedJanDec = SessionCutOff.Require(SessionType.JanDec, janDec.Month, janDec.Day);
        (AprMarCutOffMonth, AprMarCutOffDay) = (checkedAprMar.Month, checkedAprMar.Day);
        (JanDecCutOffMonth, JanDecCutOffDay) = (checkedJanDec.Month, checkedJanDec.Day);
    }

    public SessionCutOff CutOffOf(SessionType type) => type == SessionType.AprMar ? AprMar : JanDec;
}
