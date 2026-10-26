using kitsu_backend.Features.UserAnime;

namespace kitsu_backend.Tests.Features.UserAnime
{
    public class UpdateLibraryEntryUnitTests
    {
        [Theory]
        [InlineData(5, 3, 12, 5)] // Valid case: requested episodes watched is within the valid range
        [InlineData(0, 3, 12, 0)] // Valid case: requested episodes watched is zero
        [InlineData(12, 3, 12, 12)] // Valid case: requested episodes watched is equal to anime episode count
        [InlineData(-1, 3, 12, 3)] // Invalid case: requested episodes watched is negative. Stick with current episodes watched
        [InlineData(15, 3, 12, 3)] // Invalid case: requested episodes watched exceeds anime episode count. Stick with current episodes watched
        // requested, current, episodeCount, expected
        public void GetValidEpisodesWatched_ShouldReturnValidEpisodesWatched(int requested, int current, int episodeCount, int expected)
        {
            //Act 
            var result = UpdateLibraryEntryHandler.GetValidEpisodesWatched(requested, current, episodeCount);
            //Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("watching", 12, 12, "completed")] // finished progress implies completed
        [InlineData("completed", 12, 12, "completed")]
        [InlineData("watching", 5, 12, "watching")] // partial progress implies watching
        [InlineData("completed", 5, 12, "watching")]
        [InlineData("planning", 5, 12, "watching")]
        [InlineData("completed", 0, 12, "watching")] // no progress should be watching
        [InlineData("planning", 0, 12, "planning")] // Valid case: watch status is planning and episode progress is zero
        [InlineData("planning", 12, 12, "completed")] // Valid case: watch status is planning and episode progress is equal to anime episode count
        // requested, episodeProgress, episodeCount, expected
        public void GetValidWatchStatus_ShouldReturnValidWatchStatus(string requested, int episodeProgress, int episodeCount, string expected)
        {
            //Act 
            var result = UpdateLibraryEntryHandler.GetValidWatchStatus(requested, episodeProgress, episodeCount);
            //Assert
            Assert.Equal(expected, result);
        }
    }
}
