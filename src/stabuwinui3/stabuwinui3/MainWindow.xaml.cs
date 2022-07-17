using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace stabuwinui3
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        class Score
        {
            public decimal positive { get; set; }
            public decimal neutral { get; set; }
            public decimal negative { get; set; }
        }

        class Stabu
        {
            public string Id { get; set; }
            public string MediaUrl { get; set; }
            public Score Score { get; set; }
            public List<string> Objects { get; } = new();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var json = System.Text.Json.JsonSerializer.Deserialize<dynamic[]>(textBox.Text);
            IList<Stabu> stabus = ToStabus(json);
            // スコア
            decimal positive = stabus.Average(stabu => stabu.Score.positive);
            decimal neutral = stabus.Average(stabu => stabu.Score.neutral);
            decimal negative = stabus.Average(stabu => stabu.Score.negative);
            // オブジェクト
            KeyValuePair<string, int>[] objects = stabus.SelectMany(stabu => stabu.Objects).GroupBy(obj => obj).Select(objs => KeyValuePair.Create(objs.Key, objs.Count())).OrderByDescending(obj => obj.Value).ToArray();
            // HTML 化
            string html = ToHtml(stabus, positive, neutral, negative, objects);
            // ファイルに保存
            string saveFilePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Environment.ProcessPath), $"{Environment.TickCount64}.html");
            System.IO.File.WriteAllText(saveFilePath, html);
            // 画面に表示
            webView2.Source = new Uri(saveFilePath);
        }

        private IList<Stabu> ToStabus(dynamic[] json) =>
            json.Aggregate(new List<Stabu>(), (stabus, m) =>
                {
                    var value = m.ToString();
                    var stabu = stabus.LastOrDefault();
                    if ("----------" == value) { stabus.Add(new()); return stabus; }
                    if (stabu.Id == null) { stabu.Id = value; return stabus; }
                    if (stabu.MediaUrl == null) { stabu.MediaUrl = value; return stabus; }
                    if (stabu.Score == null) { stabu.Score = System.Text.Json.JsonSerializer.Deserialize<Score>(value); return stabus; }
                    stabu.Objects.Add(value);
                    return stabus;
                }
        );

        private string ToHtml(IList<Stabu> stabus, decimal positive, decimal neutral, decimal negative, IEnumerable<KeyValuePair<string, int>> objects) =>
@$"
<html><body>
positive:{positive}<br />neutral:{neutral}<br />negative:{negative}<br/><br />{string.Join("", objects.Select(obj => $"{obj.Key}:{obj.Value}<br/>"))}
<br /><table>
{string.Join("", stabus.Select(stabu => @$"
<tr><td><div style=""height: 175px; overflow:hidden;""><iframe height=350 style=""transform: scale(0.50);transform-origin: 0 0;"" src=""https://platform.twitter.com/embed/Tweet.html?id={stabu.Id}""></iframe></div></td>
<td>positive:{stabu.Score.positive}<br />neutral:{stabu.Score.neutral}<br />negative:{stabu.Score.negative}</td>
<td>{string.Join("<br />", stabu.Objects)}</td></tr>
"))}
</table>
</body></html>";

    }
}


