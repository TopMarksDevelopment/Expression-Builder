namespace TopMarksDevelopment.ExpressionBuilder.Api;

public interface IFilterableConnection
{
    IFilterable And();

    IFilterable Or();

    IFilterableConnection CloseGroup();

    IFilterableConnection CloseCollection();
}
