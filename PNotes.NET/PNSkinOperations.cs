// PNotes.NET - open source desktop notes manager
// Copyright (C) 2015 Andrey Gruber

// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Packaging;
using System.Xml;
using System.Xml.Linq;

namespace PNotes.NET
{
    internal class PNSkinsOperations
    {
        internal static void LoadSkin(string skinPath, PNSkinDetails skd)
        {
            try
            {
                //open skin file as regular zip archive
                using (var package = Package.Open(skinPath, FileMode.Open, FileAccess.Read))
                {
                    //build parameters file URI
                    var uriTarget = PackUriHelper.ResolvePartUri(new Uri("/", UriKind.Relative), new Uri("data.xml", UriKind.Relative));
                    //get parameters file
                    var documentPart = package.GetPart(uriTarget);
                    //read parameters file with XmlReader
                    var xrs = new XmlReaderSettings { IgnoreWhitespace = true };
                    using (var xr = XmlReader.Create(documentPart.GetStream(), xrs))
                    {
                        var arr = new string[] { };
                        //load parameters file into XDocument
                        var xdoc = XDocument.Load(xr);
                        //get mask color
                        var xElement = xdoc.Element("data");
                        if (xElement != null)
                        {
                            var element = xElement.Element("mask_color");
                            if (element != null)
                                arr = element.Value.Split((','));
                        }
                        skd.MaskColor = Color.FromArgb(int.Parse(arr[0], PNRuntimes.Instance.CultureInvariant), int.Parse(arr[1], PNRuntimes.Instance.CultureInvariant), int.Parse(arr[2], PNRuntimes.Instance.CultureInvariant));
                        //get delete/hide position
                        xElement = xdoc.Element("data");
                        if (xElement != null)
                        {
                            var element = xElement.Element("dimensions");
                            var element1 = element?.Element("delete_hide");
                            if (element1 != null)
                                arr = element1.Value.Split((','));
                        }
                        skd.PositionDelHide = new Rectangle(int.Parse(arr[0], PNRuntimes.Instance.CultureInvariant), int.Parse(arr[1], PNRuntimes.Instance.CultureInvariant), int.Parse(arr[2], PNRuntimes.Instance.CultureInvariant), int.Parse(arr[3], PNRuntimes.Instance.CultureInvariant));
                        //get marks position
                        xElement = xdoc.Element("data");
                        if (xElement != null)
                        {
                            var element = xElement.Element("dimensions");
                            var element1 = element?.Element("marks");
                            if (element1 != null)
                            {
                                arr = element1.Value.Split((','));
                                skd.PositionMarks = new Rectangle(int.Parse(arr[0], PNRuntimes.Instance.CultureInvariant), int.Parse(arr[1], PNRuntimes.Instance.CultureInvariant), int.Parse(arr[2], PNRuntimes.Instance.CultureInvariant), int.Parse(arr[3], PNRuntimes.Instance.CultureInvariant));
                            }
                        }

                        //get toolbar position
                        xElement = xdoc.Element("data");
                        if (xElement != null)
                        {
                            var element = xElement.Element("dimensions");
                            var element1 = element?.Element("toolbar");
                            if (element1 != null)
                            {
                                arr = element1.Value.Split((','));
                                skd.PositionToolbar = new Rectangle(int.Parse(arr[0], PNRuntimes.Instance.CultureInvariant), int.Parse(arr[1], PNRuntimes.Instance.CultureInvariant), int.Parse(arr[2], PNRuntimes.Instance.CultureInvariant), int.Parse(arr[3], PNRuntimes.Instance.CultureInvariant));
                            }
                        }

                        //get edit box position
                        xElement = xdoc.Element("data");
                        if (xElement != null)
                        {
                            var element = xElement.Element("dimensions");
                            var element1 = element?.Element("edit_box");
                            if (element1 != null)
                            {
                                arr = element1.Value.Split((','));
                                skd.PositionEdit = new Rectangle(int.Parse(arr[0], PNRuntimes.Instance.CultureInvariant), int.Parse(arr[1], PNRuntimes.Instance.CultureInvariant), int.Parse(arr[2], PNRuntimes.Instance.CultureInvariant), int.Parse(arr[3], PNRuntimes.Instance.CultureInvariant));
                            }
                        }

                        //get tooltip position
                        xElement = xdoc.Element("data");
                        if (xElement != null)
                        {
                            var element = xElement.Element("dimensions");
                            var element1 = element?.Element("tooltip");
                            if (element1 != null)
                            {
                                arr = element1.Value.Split((','));
                                skd.PositionTooltip = new Rectangle(int.Parse(arr[0], PNRuntimes.Instance.CultureInvariant), int.Parse(arr[1], PNRuntimes.Instance.CultureInvariant), int.Parse(arr[2], PNRuntimes.Instance.CultureInvariant), int.Parse(arr[3], PNRuntimes.Instance.CultureInvariant));
                            }
                        }

                        //get skin info
                        xElement = xdoc.Element("data");
                        if (xElement != null)
                        {
                            var element = xElement.Element("skin_info");
                            if (element != null)
                                skd.SkinInfo = element.Value;
                        }
                        //get possible vertical toolbar
                        xElement = xdoc.Element("data");
                        if (xElement != null)
                        {
                            var element = xElement.Element("vertical_toolbar");
                            if (element != null)
                                skd.VerticalToolbar = bool.Parse(element.Value);
                        }
                        //get marks count
                        xElement = xdoc.Element("data");
                        if (xElement != null)
                        {
                            var element = xElement.Element("dimensions");
                            var element1 = element?.Element("marks_count");
                            if (element1 != null)
                                skd.MarksCount = int.Parse(element1.Value, PNRuntimes.Instance.CultureInvariant);
                        }
                    }
                    //get scale factors
                    var factors = PNStatic.GetScalingFactors();
                    //build body image URI
                    uriTarget = PackUriHelper.ResolvePartUri(new Uri("/", UriKind.Relative), new Uri("body.png", UriKind.Relative));
                    //extract body image
                    var resourcePart = package.GetPart(uriTarget);
                    //save body image
                    skd.BitmapSkin = PNStatic.MakeTransparentBitmap(
                        Image.FromStream(resourcePart.GetStream()), skd.MaskColor);
                    //save body image for edit
                    using (
                        var bmp = resizeImage(skd.BitmapSkin, (int) (skd.BitmapSkin.Width*factors.Item1),
                            (int) (skd.BitmapSkin.Height*factors.Item2)))
                    {
                        skd.BitmapForEdit = PNStatic.MakeTransparentBitmap(bmp, skd.MaskColor);
                    }
                    //build pattern image uri
                    uriTarget = PackUriHelper.ResolvePartUri(new Uri("/", UriKind.Relative), new Uri("pattern.png", UriKind.Relative));
                    //extract pattern image
                    resourcePart = package.GetPart(uriTarget);
                    //save pattern image
                    skd.BitmapPattern =
                        PNStatic.MakeTransparentBitmap(Image.FromStream(resourcePart.GetStream()),
                            skd.MaskColor);
                    //build inactive pattern image uri
                    uriTarget = PackUriHelper.ResolvePartUri(new Uri("/", UriKind.Relative), new Uri("inactivepattern.png", UriKind.Relative));
                    if (package.PartExists(uriTarget))
                    {
                        //extract inactive pattern image
                        resourcePart = package.GetPart(uriTarget);
                        //save inactive pattern image
                        skd.BitmapInactivePattern =
                            PNStatic.MakeTransparentBitmap(Image.FromStream(resourcePart.GetStream()),
                                skd.MaskColor);
                    }
                    else
                    {
                        skd.BitmapInactivePattern = null;
                    }
                    //build delete/hide image URI
                    uriTarget = PackUriHelper.ResolvePartUri(new Uri("/", UriKind.Relative), new Uri("dh.png", UriKind.Relative));
                    //extract delete/hide image
                    resourcePart = package.GetPart(uriTarget);
                    //save delete/hide image
                    skd.BitmapDelHide =
                        PNStatic.MakeTransparentBitmap(Image.FromStream(resourcePart.GetStream()),
                            skd.MaskColor);
                    //build marks image URI
                    uriTarget = PackUriHelper.ResolvePartUri(new Uri("/", UriKind.Relative), new Uri("marks.png", UriKind.Relative));
                    //extract marks image
                    resourcePart = package.GetPart(uriTarget);
                    //save marks image
                    skd.BitmapMarks = PNStatic.MakeTransparentBitmap(
                        Image.FromStream(resourcePart.GetStream()), skd.MaskColor);
                    //build toolbar image URI
                    uriTarget = PackUriHelper.ResolvePartUri(new Uri("/", UriKind.Relative), new Uri("tbr.png", UriKind.Relative));
                    //extract toolbar image
                    resourcePart = package.GetPart(uriTarget);
                    //save toolbar image
                    skd.BitmapCommands =
                        PNStatic.MakeTransparentBitmap(Image.FromStream(resourcePart.GetStream()),
                            skd.MaskColor);
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static void ApplyNoteSkin(WndNote dlg, PNote note, bool fromLoad = false)
        {
            try
            {
                var skn = GetSkin(dlg, note);
                if (skn != null && skn.BitmapSkin != null)
                {
                    if (dlg.Equals(note.Dialog))
                    {
                        dlg.Hide();
                    }
                    dlg.SetRuntimeSkin(skn.Clone(), fromLoad);
                    if (dlg.Equals(note.Dialog))
                    {
                        dlg.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
            }
        }

        internal static PNSkinDetails GetSkin(WndNote dlg, PNote note)
        {
            try
            {
                PNSkinDetails skn;
                if (note.Skin != null)
                {
                    skn = note.Skin;
                }
                else
                {
                    var gr = PNCollections.Instance.Groups.GetGroupById(note.GroupId);
                    if (gr.Skin == null || gr.Skin.SkinName == PNSkinDetails.NO_SKIN)
                    //if (gr.ID != (int)SpecialGroups.Docking && gr.ID != (int)SpecialGroups.Diary)
                    {
                        // get General png skin
                        gr = PNCollections.Instance.Groups.GetGroupById(0);
                    }
                    skn = gr.Skin;
                }
                return skn;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return null;
            }
        }

        //[DllImport("gdi32.dll")]
        //private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        //public enum DeviceCap
        //{
        //    Vertres = 10,
        //    Desktopvertres = 117,

        //    // http://pinvoke.net/default.aspx/gdi32/GetDeviceCaps.html
        //}

        private static Bitmap resizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            try
            {
                destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                using (var graphics = Graphics.FromImage(destImage))
                {
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using (var wrapMode = new ImageAttributes())
                    {
                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                        graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                    }
                }

                return destImage;
            }
            catch (Exception ex)
            {
                PNStatic.LogException(ex);
                return destImage;
            }
        }
    }
}
