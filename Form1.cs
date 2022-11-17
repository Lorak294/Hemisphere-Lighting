using System.Diagnostics;
using System.Drawing.Printing;
using System.Globalization;
using System.Security.Cryptography;
using System.Windows.Forms.Design.Behavior;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;

namespace GKProj2
{
    //public struct DrawingArgs
    //{
    //    public Bitmap canvas;
    //    public int m;
    //    public double ks, kd;
    //    public Vert lightSource;
    //    public Color sphereColor;
    //    public Color lightColor;

    //    public DrawingArgs(Bitmap canvas)
    //}
    public partial class Form1 : Form
    {
        private Bitmap canvas;
        private List<Figure> figureList = new List<Figure>();
        //private const int MARGIN = 20;
        private double ks, kd, m;
        private int elapsedTime = 0;
        private bool vecInterpolation = true, r3interpolation = false;
        private Vert lightPosition;

        //private DrawingArgs drawingArgs;

        private Color sphereColor = Color.White;
        private Color lightColor = Color.White;
        public Form1()
        {
            InitializeComponent();

            //this.drawingArgs = new DrawingArgs()

            colorShowButton.BackColor = sphereColor;
            lightColorShowButton.BackColor = lightColor;
            ks = kd = 0.5;
            m = 50;
            
            // LIGHT ON TOP OF THE SPHERE
            lightPosition = new Vert(0, 0, lightZBar.Value / 10, new Vector(0, 0, 0));

            canvas = new Bitmap(pictureBox.Width, pictureBox.Height);
            pictureBox.Image = canvas;
            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.Clear(Color.White);
            }
            figureList = new List<Figure>();
            ImportObjFile("C:\\VS Projects\\GKProj2\\hemisphereAVG.obj");

            //figureList.Add(new Figure(new List<Vert> {
            //    new Vert(0.19509,0,0.980785, new Vector(0, 0, 1)),
            //    new Vert(0, 0, 1,  new Vector(0, 0, 1)),
            //    new Vert(0.191342,-0.03806,0.980785,  new Vector(0, 0, 1)) }));

            //figureList.Add(new Figure(new List<Vert> {
            //    new Vert(0.5,0,0, new Vector(0, 0, 1)),
            //    new Vert(0.5, 0.5, 0,  new Vector(0, 0, 1)),
            //    new Vert(-0.5,-0.3,0,  new Vector(0, 0, 1)) }));

            RenderParameters.height = canvas.Height;
            RenderParameters.width = canvas.Width;
            DrawAll();
        }

        public void DrawAll()
        {
            using(Graphics g = Graphics.FromImage(canvas))
            {
                g.Clear(Color.White);
            }

            foreach (Figure f in figureList)
            {
                f.Draw(canvas, sphereColor, netCheckBox.Checked, vecInterpolation, lightPosition,lightColor,m,kd,ks,r3interpolation);
            }
            //figureList[3].Draw(canvas, sphereColor, netCheckBox.Checked, vecInterpolation, lightPosition);
            pictureBox.Refresh();
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

        private void MoveLightSource()
        {
            (double sinT,double cosT) = Math.SinCos(elapsedTime);
            double radius = Math.Abs(Math.Sin(0.0005*elapsedTime));

            lightPosition.X = cosT * radius;
            lightPosition.Y = sinT * radius;
        }

        private void pictureBox_SizeChanged(object sender, EventArgs e)
        {
            canvas = new Bitmap(pictureBox.Size.Width, pictureBox.Size.Height);
            pictureBox.Image = canvas;
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
                sphereColor = MyDialog.Color;
                colorShowButton.BackColor = sphereColor;
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
            kd = (float)kdBar.Value / 100;
            DrawAll();
        }

        private void lightColorButton_Click(object sender, EventArgs e)
        {
            ColorDialog MyDialog = new ColorDialog();
            MyDialog.AllowFullOpen = false;
            MyDialog.Color = Color.White;
            if (MyDialog.ShowDialog() == DialogResult.OK)
            {
                lightColor = MyDialog.Color;
                lightColorShowButton.BackColor = lightColor;
            }
            DrawAll();
        }

        private void vecIntRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            vecInterpolation = vecIntRadioButton.Checked;
            DrawAll();
        }

        private void lightZBar_Scroll(object sender, EventArgs e)
        {
            lightZValueLabel.Text = ((double)lightZBar.Value / 10).ToString();
            lightPosition.Z = (double)lightZBar.Value / 10;
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
            r3interpolation = r3RadioButton.Checked;
            DrawAll();
        }

        private void mBar_Scroll(object sender, EventArgs e)
        {
            mValueLabel.Text = mBar.Value.ToString();
            m = mBar.Value;
            DrawAll();
        }

        private void ksBar_Scroll(object sender, EventArgs e)
        {
            ksValueLabel.Text = ((double)ksBar.Value / 100).ToString();
            ks = (float)ksBar.Value / 100;
            DrawAll();
        }
    }
}