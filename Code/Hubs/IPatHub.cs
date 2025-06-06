using System.Threading.Tasks;

namespace watchtower.Code.Hubs {

    public interface IPatHub {

        Task SendValue(long value);

    }
}
