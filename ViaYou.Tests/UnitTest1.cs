using Xunit;
using ViaYou.Core.Entities;
using System;

namespace ViaYou.Tests
{
    public class GoalEntityTests
    {
        [Fact]
        public void Goal_Initialization_ShouldSetCreatedAtToCurrentTime()
        {
            // Arrange & Act
            var goal = new Goal();

            // Assert
            var difference = DateTime.Now - goal.CreatedAt;
            Assert.True(difference.TotalSeconds < 2, "CreatedAt should immediately default to the current time.");
        }

        [Fact]
        public void Goal_TargetAmount_ShouldBeAccuratelyAssigned()
        {
            // Arrange
            decimal targetAmount = 250000.50m;
            decimal monthlyContribution = 10000.00m;

            // Act
            var goal = new Goal 
            { 
                Name = "Dream Car", 
                TargetAmount = targetAmount, 
                MonthlyContribution = monthlyContribution 
            };

            // Assert
            Assert.Equal("Dream Car", goal.Name);
            Assert.Equal(250000.50m, goal.TargetAmount);
            Assert.Equal(10000.00m, goal.MonthlyContribution);
        }
    }
}
