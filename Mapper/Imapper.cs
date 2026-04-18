namespace Stock_Server.Mapper;

public interface IMapper<TSource, TDestination>
{
    TDestination Map(TSource source);
    TSource ReverseMap(TDestination destination);
}