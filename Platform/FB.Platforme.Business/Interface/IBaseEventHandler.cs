namespace FB.Platforme.Business.Interface;

public interface IBaseEventHandler
{
    Task Handle(object payload);
}
