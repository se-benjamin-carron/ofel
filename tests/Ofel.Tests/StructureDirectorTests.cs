using System;
using Xunit;
#nullable enable

namespace Ofel.Tests
{
    public class StructureDirectorTests : BaseTestWithResults
    {
        [Fact]
        public void CreateDefault4MemberFrame_ComputesWithoutException()
        {
            // Arrange
            var structure = ofel.Core.StructureDirector.CreateDefault4MemberFrame(span: 3.0, height: 2.0, mesh: 0.5f);

            // Act
            Exception? ex = null;
            try
            {
                structure.Compute();
            }
            catch (Exception e)
            {
                ex = e;
            }

            // Assert
            Assert.Null(ex);
            Assert.Equal(4, structure.FiniteElementMembers.Count);
            Assert.True(structure.PointsStructureData.Count > 0);

            // record a short summary in the shared results file
            WriteResults(new string[] { "CreateDefault4MemberFrame" }, new string[] { ex == null ? "OK" : ex.Message }, "StructureDirectorTests.CreateDefault4MemberFrame");
        }
    }
}
