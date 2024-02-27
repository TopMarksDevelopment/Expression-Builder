namespace TopMarksDevelopment.ExpressionBuilder.Api;

public interface IFilterConnection
{
    IFilter And();
    
    IFilter Or();

    IFilterConnection CloseGroup();

    IFilterConnection CloseCollection();
}