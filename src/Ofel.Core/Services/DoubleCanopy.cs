//public sealed class GantryInput
//{
//    public double HeightUnderBeam { get; init; }
//    public double AdditionnalHeightBeam { get; init; }
//    public double FootingDepth { get; init; }
//    public double Slope { get; init; }
//    public double CantileverIntegrationSystemRight { get; init; }
//    public double CantileverIntegrationSystemLeft { get; init; }
//    public double ShiftRightBrace { get; init; }
//    public double ShiftLeftBrace { get; init; }
//    public double HeightBraceColumn { get; init; }
//    public double ShiftColumn { get; init; }
//}



//public sealed class GantryDirector
//{
//    private readonly IGantryBuilder _builder;

//    public GantryDirector(IGantryBuilder builder)
//    {
//        _builder = builder;
//    }

//    public IReadOnlyList<Member> Build(GantryInput input)
//    {
//        _builder.Reset();

//        _builder.BuildColumns(input);
//        _builder.BuildBeams(input);
//        _builder.ApplyBracing(input);

//        return _builder.GetMembers();
//    }
//}
