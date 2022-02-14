using Microsoft.Win32;
using System;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading.Tasks;

namespace InkCanvas
{
    public partial class InkPadWindow
    {
        // Make the pad 4 inches by 5 inches.
        public static readonly double widthCanvas = 8 * 96;
        public static readonly double heightCanvas = 5 * 96;

        public InkPadWindow()
        {
            this.InitializeComponent();
            this.radInk.IsChecked = true;
        }

        // New command: just clear all the strokes.
        private void btnNew_Click(object sender, RoutedEventArgs args)
        {
            this.inkCanv.Strokes.Clear();
        }

        // Open command: display OpenFileDialog and load ISF file.
        private async void btnOpen_Click(object sender, RoutedEventArgs args)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.CheckFileExists = true;
            dlg.Filter = "Ink Serialized Format (*.isf)|*.isf|" +
                         "All files (*.*)|*.*";

            if ((bool)dlg.ShowDialog(this))
            {
                this.inkCanv.Strokes.Clear();

                try
                {
                    using (FileStream file = new FileStream(dlg.FileName,
                                                FileMode.Open, FileAccess.Read))
                    {
                        if (!dlg.FileName.ToLower().EndsWith(".isf"))
                        {
                            MessageBox.Show("The requested file is not a Ink Serialized Format file\r\n\r\nplease retry", Title);
                        }
                        else
                        {
                            var c = new StrokeCollection(file);
                            this.inkCanv.Strokes = new StrokeCollection();
                            foreach (var item in c)
                            {
                                var spc = new StylusPointCollection();
                                spc.Add(item.StylusPoints.First());
                                var stroke = new Stroke(spc, item.DrawingAttributes);

                                await App.Current.Dispatcher.Invoke(
                                    async () =>
                                    {

                                        this.inkCanv.Strokes.Add(stroke);

                                        await Task.Run(() =>
                                        {
                              
                                            //for (int i = 1; i < item.StylusPoints.Count; i++)
                                    //{
                                    //    App.Current.Dispatcher.Invoke(() =>
                                    //    {
                                    //        stroke.StylusPoints = new StylusPointCollection(item.StylusPoints.Take(i).ToArray());

                                    //    });
                                    //    Task.Delay(0).Wait();
                                    //}


                                    foreach (var sp in item.StylusPoints.Skip(1))
                                            {

                                                App.Current.Dispatcher.Invoke(() =>
                                                   {
                                                       stroke.StylusPoints.Add(sp);
                                                   });
                                                //Task.Delay(1).Wait();
                                                //for (int i = 0; i < 1000; i++)
                                                {
                                                    System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(1));
                                                }
                                            }

                                        }).ConfigureAwait(false);
                                    }
                                ).ConfigureAwait(false);
                            }
                            file.Close();
                        }
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message, Title);
                }
            }
        }


        // File Save : display SaveFileDialog.
        private void btnSave_Click(object sender, RoutedEventArgs args)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Ink Serialized Format (*.isf)|*.isf|" +
                         "Bitmap files (*.bmp)|*.bmp";

            if ((bool)dlg.ShowDialog(this))
            {
                try
                {
                    using (FileStream file = new FileStream(dlg.FileName,
                                            FileMode.Create, FileAccess.Write))
                    {
                        //Ink Serialized Format
                        if (dlg.FilterIndex == 1)
                        {
                            this.inkCanv.Strokes.Save(file);
                            file.Close();
                        }
                        //bitmap object
                        else
                        {
                            int marg = int.Parse(this.inkCanv.Margin.Left.ToString());
                            RenderTargetBitmap rtb = new RenderTargetBitmap((int)this.inkCanv.ActualWidth - marg,
                                            (int)this.inkCanv.ActualHeight - marg, 0, 0, PixelFormats.Default);
                            rtb.Render(this.inkCanv);
                            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(rtb));
                            encoder.Save(file);
                            file.Close();
                        }
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message, Title);
                }
            }
        }

        // Cut command: cut all selected strokes
        private void btnCut_Click(object sender, RoutedEventArgs args)
        {
            if (this.inkCanv.GetSelectedStrokes().Count > 0)
                this.inkCanv.CutSelection();
        }

        // Copy command: copy all selected strokes
        private void btnCopy_Click(object sender, RoutedEventArgs args)
        {
            if (this.inkCanv.GetSelectedStrokes().Count > 0)
                this.inkCanv.CopySelection();
        }

        // Paste command: paste all selected strokes
        private void btnPaste_Click(object sender, RoutedEventArgs args)
        {
            if (this.inkCanv.CanPaste())
                this.inkCanv.Paste();
        }

        // Delete command: delete all selected strokes
        private void btnDelete_Click(object sender, RoutedEventArgs args)
        {
            if (this.inkCanv.GetSelectedStrokes().Count > 0)
            {
                foreach (Stroke strk in this.inkCanv.GetSelectedStrokes())
                    this.inkCanv.Strokes.Remove(strk);
            }
        }

        // SelectAll command: select all strokes
        private void btnSelectAll_Click(object sender, RoutedEventArgs args)
        {
            this.inkCanv.Select(this.inkCanv.Strokes);
        }

        private void rad_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rad = sender as RadioButton;
            this.inkCanv.EditingMode = (InkCanvasEditingMode)rad.Tag;
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void penSize_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rad = sender as RadioButton;
            DrawingAttributes inkDA = new DrawingAttributes();
            inkDA.Width = rad.FontSize;
            inkDA.Height = rad.FontSize;
            inkDA.Color = this.inkCanv.DefaultDrawingAttributes.Color;
            inkDA.IsHighlighter = this.inkCanv.DefaultDrawingAttributes.IsHighlighter;
            this.inkCanv.DefaultDrawingAttributes = inkDA;
            this.expB.IsExpanded = false;
        }

        private void btnStylusSettings_Click(object sender, RoutedEventArgs e)
        {
            StylusSettings dlg = new StylusSettings();
            dlg.Owner = this;
            dlg.DrawingAttributes = this.inkCanv.DefaultDrawingAttributes;
            if ((bool)dlg.ShowDialog().GetValueOrDefault())
            {
                this.inkCanv.DefaultDrawingAttributes = dlg.DrawingAttributes;
            }
        }

        private void btnFormat_Click(object sender, RoutedEventArgs e)
        {
            StylusSettings dlg = new StylusSettings();
            dlg.Owner = this;

            // Try getting the DrawingAttributes of the first selected stroke.
            StrokeCollection strokes = this.inkCanv.GetSelectedStrokes();

            if (strokes.Count > 0)
                dlg.DrawingAttributes = strokes[0].DrawingAttributes;
            else
                dlg.DrawingAttributes = this.inkCanv.DefaultDrawingAttributes;

            if ((bool)dlg.ShowDialog().GetValueOrDefault())
            {
                // Set the DrawingAttributes of all the selected strokes.
                foreach (Stroke strk in strokes)
                    strk.DrawingAttributes = dlg.DrawingAttributes;
            }
        }
    }
}
