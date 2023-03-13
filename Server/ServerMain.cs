using System.Globalization;
using static System.DateTime;
using CitizenFX.Core;

namespace geneva_television.Server
{
    public class ServerMain : BaseScript
    {
        [EventHandler("geneva-television:initTelevision")]
        private void OnInitTelevision([FromSource] Player player, int netId)
        {
            Entity television = Entity.FromNetworkId(netId);
            if (television is null) return;
            Debug.WriteLine($"[^3{Now.ToString("G", CultureInfo.InvariantCulture)}^0] Initializing statebags for television (^3NetID: {netId.ToString()}^0)");
            television.State.Set("beingUsed", false,  true);
        }
    }
}