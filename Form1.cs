using System.Diagnostics;
using System.Drawing.Printing;
using System.Globalization;
using System.Security.Cryptography;
using System.Windows.Forms.Design.Behavior;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;

namespace GKProj2
{
    public partial class Form1 : Form
    {
        private Bitmap canvas;
        private Bitmap texture;
        private Bitmap normalMap;
        private LockBitmap lockBitmapCanvas;

        private List<Figure> figureList = new List<Figure>();
        private int elapsedTime = 0;


        public Form1()
        {
            InitializeComponent();

            colorShowButton.BackColor = Color.White;
            lightColorShowButton.BackColor = Color.White;

            canvas = new Bitmap(pictureBox.Width, pictureBox.Height);
            pictureBox.Image = canvas;
            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.Clear(Color.White);
            }
            lockBitmapCanvas = new LockBitmap(canvas);

            texture = new Bitmap("../../../defaultTexture.png");
            textureShowButton.BackgroundImage = texture;
            using (Graphics g = Graphics.FromImage(textureShowButton.BackgroundImage))
            {
                g.DrawImage(textureShowButton.BackgroundImage, textureShowButton.DisplayRectangle);
            }

            normalMap = new Bitmap("../../../defaultNormalMap.png");
            normalMapShowButton.BackgroundImage = normalMap;
            using (Graphics g = Graphics.FromImage(normalMapShowButton.BackgroundImage))
            {
                g.DrawImage(normalMapShowButton.BackgroundImage, normalMapShowButton.DisplayRectangle);
            }

            DrawingArgs.textureLockBitmap = new LockBitmap(texture);
            DrawingArgs.normalMapLockBitmap = new LockBitmap(normalMap);
            DrawingArgs.lightPosition = new Vert(0, 0, lightZBar.Value / 10, new Vector(0, 0, 0));
            DrawingArgs.lightColor = Color.White;
            DrawingArgs.sphereColor = Color.White;
            DrawingArgs.m = mBar.Value;
            DrawingArgs.kd = (double)kdBar.Value / 100;
            DrawingArgs.ks = (double)ksBar.Value / 100;
            DrawingArgs.r3 = r3RadioButton.Checked;
            DrawingArgs.vecInterpolation = vecIntRadioButton.Checked;
            DrawingArgs.textureDraw = textureRadioButton.Checked;
            DrawingArgs.useNormalMap = normalMapCheckBox.Checked;

            RenderParameters.height = canvas.Height;
            RenderParameters.width = canvas.Width;

            figureList = new List<Figure>();
            ImportObjFile("../../../hemisphereAVG.obj");

            DrawAll();
        }
        public void DrawAll()
        {
            var sw = Stopwatch.StartNew();
            
            lockBitmapCanvas.LockBits();
            if(DrawingArgs.textureDraw)DrawingArgs.textureLockBitmap!.LockBits();
            if (DrawingArgs.useNormalMap) DrawingArgs.normalMapLockBitmap!.LockBits();
            foreach (Figure f in figureList)
            {
                f.FillTriangle(lockBitmapCanvas);
            }
            if (DrawingArgs.useNormalMap) DrawingArgs.normalMapLockBitmap!.UnlockBits();
            if (DrawingArgs.textureDraw) DrawingArgs.textureLockBitmap!.UnlockBits();
            lockBitmapCanvas.UnlockBits();
            
            if (netCheckBox.Checked)
            {
                foreach(Figure f in figureList)
                {
                    f.DrawOutline(canvas);
                }
            }
            pictureBox.Refresh();

            sw.Stop();
            Debug.WriteLine(sw.ElapsedMilliseconds.ToString());
        }

        // OBJ FILE IMPORT
        public void ImportObjFile(string filepath)
        {
            List<Vert> vertList = new List<Vert>();
            List<Vector> normVectorList = new List<Vector>();

            FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            sr.BaseStream.Seek(0, SeekOrigin.Begin);

            string lineStr = sr.ReadLine()!;

            // importing verts and normal vectors
            while(lineStr != null && !lineStr.StartsWith('f'))
            {
                try
                {
                    if (lineStr.StartsWith("vn"))
                    {
                        normVectorList.Add(ParseNormVector(lineStr));
                    }
                    else if (lineStr.StartsWith("v"))
                    {
                        vertList.Add(new Vert(lineStr));
                    }
                    lineStr = sr.ReadLine()!;
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
                
            }

            // importing figures
            while (lineStr != null && lineStr.StartsWith('f'))
            {
                //Debug.WriteLine(lineStr);
                try
                {
                    figureList.Add(ParseFigure(lineStr, vertList, normVectorList));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }

                lineStr = sr.ReadLine()!;
            }

            sr.Close();
            fs.Close();
        }
        public Vector ParseNormVector(string normVectorStr)
        {
            string[] args = normVectorStr.Split(' ');

            if (args[0] != "vn")
                throw new ArgumentException("Wrong string psssed to Norm Vector Parser.");

            float x, y, z;

            if (!float.TryParse(args[1], NumberStyles.Any, CultureInfo.InvariantCulture, out x))
                throw new ArgumentException("Wrong string psssed to Norm Vector Parser.");
            if (!float.TryParse(args[2], NumberStyles.Any, CultureInfo.InvariantCulture, out y))
                throw new ArgumentException("Wrong string psssed to Norm Vector Parser.");
            if (!float.TryParse(args[3], NumberStyles.Any, CultureInfo.InvariantCulture, out z))
                throw new ArgumentException("Wrong string psssed to Norm Vector Parser.");

            return new Vector(x, y, z);
        }
        public Figure ParseFigure(string figureStr, List<Vert> vertList, List<Vector> normVectorList)
        {
            List<Vert> selectedVerts = new List<Vert>();

            string[] args = figureStr.Split(' ');

            foreach (string arg in args)
            {
                if (arg == "f")
                    continue;
                string[] vertArg = arg.Split('/');

                int vertIdx, normVectorIdx;

                if (!(int.TryParse(vertArg[0],out vertIdx) && int.TryParse(vertArg[2],out normVectorIdx)))
                {
                    throw new ArgumentException("wrong string - int parsing problem");
                }

                selectedVerts.Add(new Vert(vertList[vertIdx-1], normVectorList[normVectorIdx-1]));
            }

            return new Figure(selectedVerts);
        }
        
        // LIGHT ANIMATION 
        private void MoveLightSource()
        {
            (double sinT,double cosT) = Math.SinCos(elapsedTime);
            double radius = Math.Abs(Math.Sin(0.0005*elapsedTime));

            DrawingArgs.lightPosition!.X = cosT * radius;
            DrawingArgs.lightPosition!.Y = sinT * radius;
        }
        
        // UI EVENTS
        private void pictureBox_SizeChanged(object sender, EventArgs e)
        {
            canvas = new Bitmap(pictureBox.Size.Width, pictureBox.Size.Height);
            pictureBox.Image = canvas;
            lockBitmapCanvas = new LockBitmap(canvas);
            RenderParameters.height = canvas.Height;
            RenderParameters.width = canvas.Width;
            DrawAll();
        }
        private void chooseColor_Click(object sender, EventArgs e)
        {
            ColorDialog MyDialog = new ColorDialog();
            // Keeps the user from selecting a custom color.
            MyDialog.AllowFullOpen = false;

            // Sets the initial color select to the current text color.
            MyDialog.Color = Color.White;

            // Update the text box color if the user clicks OK 
            if (MyDialog.ShowDialog() == DialogResult.OK)
            {
                DrawingArgs.sphereColor = MyDialog.Color;
                colorShowButton.BackColor = DrawingArgs.sphereColor;
            }
            DrawAll();
        }
        private void netCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            DrawAll();
        }
        private void kdBar_Scroll(object sender, EventArgs e)
        {
            kdValueLabel.Text = ((double)kdBar.Value / 100).ToString();
            DrawingArgs.kd = (float)kdBar.Value / 100;
            DrawAll();
        }
        private void lightColorButton_Click(object sender, EventArgs e)
        {
            using (ColorDialog MyDialog = new ColorDialog())
            {
                MyDialog.AllowFullOpen = false;
                MyDialog.Color = Color.White;
                if (MyDialog.ShowDialog() == DialogResult.OK)
                {
                    DrawingArgs.lightColor = MyDialog.Color;
                    lightColorShowButton.BackColor = DrawingArgs.lightColor;
                }
                DrawAll();
            }
        }
        private void vecIntRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            DrawingArgs.vecInterpolation = vecIntRadioButton.Checked;
            DrawAll();
        }
        private void lightZBar_Scroll(object sender, EventArgs e)
        {
            lightZValueLabel.Text = ((double)lightZBar.Value / 10).ToString();
            DrawingArgs.lightPosition!.Z = (double)lightZBar.Value / 10;
            DrawAll();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            elapsedTime += timer1.Interval/2;
            
            MoveLightSource();
            DrawAll();
        }
        private void animationCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (animationCheckBox.Checked)
                timer1.Start();
            else
                timer1.Stop();
        }
        private void r2RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            DrawingArgs.r3 = r3RadioButton.Checked;
            DrawAll();
        }
        private void mBar_Scroll(object sender, EventArgs e)
        {
            mValueLabel.Text = mBar.Value.ToString();
            DrawingArgs.m = mBar.Value;
            DrawAll();
        }
        private void ksBar_Scroll(object sender, EventArgs e)
        {
            ksValueLabel.Text = ((double)ksBar.Value / 100).ToString();
            DrawingArgs.ks = (double)ksBar.Value / 100;
            DrawAll();
        }
        private void sphereTextureButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Select Texture";
                dlg.Filter = "Image files |*.bmp;*.png";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    texture.Dispose();
                    texture = new Bitmap(dlg.FileName);
                    textureShowButton.BackgroundImage = texture;
                    DrawingArgs.textureLockBitmap = new LockBitmap(texture);
  
                    using(Graphics g = Graphics.FromImage(textureShowButton.BackgroundImage))
                    {
                        g.DrawImage(textureShowButton.BackgroundImage, textureShowButton.DisplayRectangle);
                    }
                }
            }
            DrawAll();
            
        }
        private void chooseColorRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (chooseColorRadioButton.Checked)
            {
                sphereColorButton.Enabled = true;
                sphereTextureButton.Enabled = false;
                DrawingArgs.textureDraw = false;
            }
            else
            {
                sphereColorButton.Enabled = false;
                sphereTextureButton.Enabled = true;
                DrawingArgs.textureDraw = true;
            }
            DrawAll();
        }
        private void normalMapButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Select Normal Map";
                dlg.Filter = "Image files |*.bmp;*.png";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    normalMap.Dispose();
                    normalMap = new Bitmap(dlg.FileName);
                    normalMapShowButton.BackgroundImage = normalMap;
                    DrawingArgs.normalMapLockBitmap = new LockBitmap(normalMap);

                    using (Graphics g = Graphics.FromImage(normalMapShowButton.BackgroundImage))
                    {
                        g.DrawImage(normalMapShowButton.BackgroundImage, normalMapShowButton.DisplayRectangle);
                    }
                }
            }
            DrawAll();
        }
        private void normalMapCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if(normalMapCheckBox.Checked)
            {
                normalMapButton.Enabled = true;
                DrawingArgs.useNormalMap = true;
            }
            else
            {
                normalMapButton.Enabled=false; 
                DrawingArgs.useNormalMap = false;
            }
            DrawAll();
        }
    }
}