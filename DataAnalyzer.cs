using System.Collections.Generic;
using System.Data;
using System.Linq;

public class DataAnalyzer
{
    private readonly List<VideoData> videos = new List<VideoData>();

    public void AddDataTable(DataTable table, string region)
    {
        foreach (DataRow row in table.Rows)
        {
            var video = new VideoData
            {
                VideoId = row["video_id"].ToString(),
                Title = row["title"].ToString(),
                Region = region,
                Views = long.TryParse(row["views"].ToString(), out long views) ? views : 0
            };
            videos.Add(video);
        }
    }

    public VideoData GetMostPopularVideo()
    {
        return videos.OrderByDescending(v => v.Views).FirstOrDefault();
    }

    public VideoData GetLeastPopularVideo()
    {
        return videos.OrderBy(v => v.Views).FirstOrDefault();
    }

    public void AddVideos(List<VideoData> videoDataList)
    {
        videos.AddRange(videoDataList);
    }
}
