using TinyMetroWpfLibrary.Controller;

namespace LOUV.Torp.ICore
{
    public interface IUnitKernel:IKernel
    {
            IMessageController MessageController { get; }
        
    }
}