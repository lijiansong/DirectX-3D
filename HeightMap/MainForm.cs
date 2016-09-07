using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace HeightMap
{
    public partial class MainForm : Form
    {
        Device m_device = null;

        private VertexBuffer vertextBuffer;//定义顶点缓冲变量
        private IndexBuffer indexBuffer;//定义索引缓冲变量
        private int[] indices;//定义索引号变量

        private CustomVertex.PositionColored[] vertices;//定义顶点变量

        private int xCount = 5, yCount = 4;//定义横向和纵向网格数目
        private float cellHeight = 1f, cellWidth = 1f;//定义单元的宽度和长度

        public Vector3 CamPostion = new Vector3(0, 100, 100);//定义摄像机位置
        public Vector3 CamTarget = new Vector3(125, 30, 125);//定义摄像机目标位置

        private float angleY = 0.01f;//定义绕Y轴旋转变量

        private int mouseLastX, mouseLastY;//记录鼠标按下时的坐标位置
        private bool isRotateByMouse = false;//记录是否由鼠标控制旋转
        private bool isMoveByMouse = false;//记录是否由鼠标控制移动

        public MainForm()
        {
            InitializeComponent();
        }
        public bool InitializeGraphics()
        {
            try
            {
                PresentParameters presentParams = new PresentParameters();
                presentParams.Windowed = true;
                presentParams.SwapEffect = SwapEffect.Discard;
                presentParams.AutoDepthStencilFormat = DepthFormat.D16;
                presentParams.EnableAutoDepthStencil = true;
                presentParams.PresentationInterval = PresentInterval.Immediate;
                m_device = new Device(0, DeviceType.Hardware, this, CreateFlags.SoftwareVertexProcessing, presentParams);
                m_device.DeviceReset += new System.EventHandler(this.OnResetDevice);
                this.OnResetDevice(m_device, null);

                VertexDeclaration();//定义顶点
                IndicesDeclaration();//定义索引缓冲
            }
            catch (DirectXException e)
            {
                MessageBox.Show(e.ToString(), "Error");
                return false;
            }
            return true;
        }
        public void OnResetDevice(object sender,EventArgs e)
        {
            Device dev = (Device)sender;
            dev.RenderState.CullMode = Cull.None;
            dev.RenderState.FillMode = FillMode.WireFrame;//设置以三角网的形式显示
            dev.RenderState.Lighting = false;
            //SetupMatrices();
        }
        private void SetupMatrices()
        {
            Matrix viewMatrix = Matrix.LookAtLH(
                CamPostion,
                CamTarget,
                new Vector3(0.0f, 1.0f, 0.0f));
            m_device.Transform.Projection = Matrix.PerspectiveFovLH(
                (float)Math.PI / 4,
                this.Width / this.Height,
                0.3f, 500f);
            m_device.Transform.View = viewMatrix;
        }
        /**
         * 通过Bitmap类获得BMP图片对象，然后根据其Width属性和Height属性设置生成三角网中横向和纵向单元数，
         * 同时设置单元尺寸，在设置高程值中通过获取图片中相应位置的颜色值，
         * 然后通过一个相对比例得到一个相对的高程值，最后将该值赋予定义的顶点坐标值
         */
        private void VertexDeclaration()
        {
            string szBitmapPath = @"F:\\workdir\\VC# Based DirectX\\heightMap.BMP";
            Bitmap bitMap = new Bitmap(szBitmapPath);
            xCount = (bitMap.Width - 1) / 2;
            yCount = (bitMap.Height - 1) / 2;
            cellWidth = bitMap.Width / xCount;
            cellHeight = bitMap.Height / yCount;

            vertextBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored),
                (xCount + 1) * (yCount + 1),
                m_device,
                Usage.Dynamic | Usage.WriteOnly,
                CustomVertex.PositionColored.Format,
                Pool.Default);
            vertices = new CustomVertex.PositionColored[(xCount + 1) * (yCount + 1)];
            for(int i=0;i<yCount+1;i++)
                for(int j=0;j<xCount+1;j++)
                {
                    Color color = bitMap.GetPixel((int)(j * cellWidth), (int)(i * cellHeight));
                    float height = float.Parse(color.R.ToString()) + float.Parse(color.G.ToString()) + float.Parse(color.B.ToString());
                    height /= 10;
                    vertices[j + i * (xCount + 1)].Position = new Vector3(j * cellWidth, height, i * cellHeight);
                    vertices[j + i * (xCount + 1)].Color = Color.White.ToArgb();
                }
            vertextBuffer.SetData(vertices,0,LockFlags.None);
        }
        private void IndicesDeclaration()//定义索引
        {
            indexBuffer = new IndexBuffer(typeof(int),
                6 * xCount * yCount,
                m_device,
                Usage.WriteOnly,
                Pool.Default);
            indices = new int[6 * xCount * yCount];
            for (int i = 0; i < yCount; i++)
            {
                for (int j = 0; j < xCount; j++)
                {
                    indices[6 * (j + i * xCount)] = j + i * (xCount + 1);
                    indices[6 * (j + i * xCount) + 1] = j + (i + 1) * (xCount + 1);
                    indices[6 * (j + i * xCount) + 2] = j + i * (xCount + 1) + 1;
                    indices[6 * (j + i * xCount) + 3] = j + i * (xCount + 1) + 1;
                    indices[6 * (j + i * xCount) + 4] = j + (i + 1) * (xCount + 1);
                    indices[6 * (j + i * xCount) + 5] = j + (i + 1) * (xCount + 1) + 1;
                }
            }
            indexBuffer.SetData(indices, 0, LockFlags.None);
        }
        public void Render()
        {
            if (m_device == null)
                return;
//             Matrix viewMatrix = Matrix.LookAtLH(
//                 CamPostion, 
//                 CamTarget, 
//                 new Vector3(0.0f, 1.0f, 0.0f));
//             m_device.Transform.Projection = Matrix.PerspectiveFovLH(
//                 (float)Math.PI / 4, 
//                 this.Width / this.Height,
//                 0.3f, 500f);
//             m_device.Transform.View = viewMatrix;
            SetupMatrices();
            m_device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.DarkSlateBlue, 1.0f, 0);
            m_device.BeginScene();

            m_device.VertexFormat = CustomVertex.PositionColored.Format;
            m_device.SetStreamSource(0, vertextBuffer, 0);
            m_device.Indices = indexBuffer;
            m_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, (xCount + 1) * (yCount + 1), 0, indices.Length / 3);
            m_device.EndScene();
            m_device.Present();
        }


        private void Form_OnLoad(object sender, EventArgs e)
        {
            InitializeGraphics();
            this.Show();
            Render();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            Vector4 tempV4;
            Matrix currentView = m_device.Transform.View;//当前摄像机的视图矩阵
            switch(e.KeyCode)
            {
                case Keys.Left:
                    CamPostion.Subtract(CamTarget);
                    tempV4 = Vector3.Transform(
                        CamPostion,
                        Matrix.RotationQuaternion
                        (
                        Quaternion.RotationAxis(
                        new Vector3(currentView.M12,
                            currentView.M22,
                            currentView.M32),-angleY)
                            )
                            );
                    CamPostion.X += tempV4.X + CamTarget.X;
                    CamPostion.Y = tempV4.Y + CamTarget.Y;
                    CamPostion.Z = tempV4.Z + CamTarget.Z;
                    break;
                case Keys.Right:
                    CamPostion.Subtract(CamTarget);
                    tempV4 = Vector3.Transform(CamPostion, Matrix.RotationQuaternion(
                            Quaternion.RotationAxis(new Vector3(currentView.M12, currentView.M22, currentView.M32), angleY)));
                    CamPostion.X = tempV4.X + CamTarget.X;
                    CamPostion.Y = tempV4.Y + CamTarget.Y;
                    CamPostion.Z = tempV4.Z + CamTarget.Z;
                    break;
                case Keys.Up:
                    CamPostion.Subtract(CamTarget);
                    tempV4 = Vector3.Transform(CamPostion, Matrix.RotationQuaternion(
                       Quaternion.RotationAxis(new Vector3(m_device.Transform.View.M11
                       , m_device.Transform.View.M21, m_device.Transform.View.M31), -angleY)));
                    CamPostion.X = tempV4.X + CamTarget.X;
                    CamPostion.Y = tempV4.Y + CamTarget.Y;
                    CamPostion.Z = tempV4.Z + CamTarget.Z;
                    break;
                case Keys.Down:
                    CamPostion.Subtract(CamTarget);
                    tempV4 = Vector3.Transform(CamPostion, Matrix.RotationQuaternion(
                       Quaternion.RotationAxis(new Vector3(m_device.Transform.View.M11
                       , m_device.Transform.View.M21, m_device.Transform.View.M31), angleY)));
                    CamPostion.X = tempV4.X + CamTarget.X;
                    CamPostion.Y = tempV4.Y + CamTarget.Y;
                    CamPostion.Z = tempV4.Z + CamTarget.Z;
                    break;
                case Keys.Add:
                    CamPostion.Subtract(CamTarget);
                    CamPostion.Scale(0.95f);
                    CamPostion.Add(CamTarget);
                    break;
                case Keys.Subtract:
                    CamPostion.Subtract(CamTarget);
                    CamPostion.Scale(1.05f);
                    CamPostion.Add(CamTarget);
                    break;
            }
            Matrix viewMatrix = Matrix.LookAtLH(CamPostion, CamTarget, new Vector3(0, 1, 0));
            m_device.Transform.View = viewMatrix;
            Render();
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseLastX = e.X;
                mouseLastY = e.Y;
                isRotateByMouse = true;
            }
            else if (e.Button == MouseButtons.Middle)
            {
                mouseLastX = e.X;
                mouseLastY = e.Y;
                isMoveByMouse = true;
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            isRotateByMouse = false;
            isMoveByMouse = false;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (isRotateByMouse)
            {
                Matrix currentView = m_device.Transform.View;//当前摄像机的视图矩阵
                float tempAngleY = 2 * (float)(e.X - mouseLastX) / this.Width;
                CamPostion.Subtract(CamTarget);

                Vector4 tempV4 = Vector3.Transform(CamPostion, Matrix.RotationQuaternion(
                    Quaternion.RotationAxis(new Vector3(currentView.M12, currentView.M22, currentView.M32), tempAngleY)));
                CamPostion.X = tempV4.X;
                CamPostion.Y = tempV4.Y;
                CamPostion.Z = tempV4.Z;

                float tempAngleX = 4 * (float)(e.Y - mouseLastY) / this.Height;
                tempV4 = Vector3.Transform(CamPostion, Matrix.RotationQuaternion(
                    Quaternion.RotationAxis(new Vector3(currentView.M11, currentView.M21, currentView.M31), tempAngleX)));
                CamPostion.X = tempV4.X + CamTarget.X;
                CamPostion.Y = tempV4.Y + CamTarget.Y;
                CamPostion.Z = tempV4.Z + CamTarget.Z;
                Matrix viewMatrix = Matrix.LookAtLH(CamPostion, CamTarget, new Vector3(0, 1, 0));
                m_device.Transform.View = viewMatrix;

                mouseLastX = e.X;
                mouseLastY = e.Y;
                Render();
            }
            else if (isMoveByMouse)
            {
                Matrix currentView = m_device.Transform.View;//当前摄像机的视图矩阵
                float moveFactor = 0.01f;
                CamTarget.X += -moveFactor * ((e.X - mouseLastX) * currentView.M11 - (e.Y - mouseLastY) * currentView.M12);
                CamTarget.Y += -moveFactor * ((e.X - mouseLastX) * currentView.M21 - (e.Y - mouseLastY) * currentView.M22);
                CamTarget.Z += -moveFactor * ((e.X - mouseLastX) * currentView.M31 - (e.Y - mouseLastY) * currentView.M32);

                CamPostion.X += -moveFactor * ((e.X - mouseLastX) * currentView.M11 - (e.Y - mouseLastY) * currentView.M12);
                CamPostion.Y += -moveFactor * ((e.X - mouseLastX) * currentView.M21 - (e.Y - mouseLastY) * currentView.M22);
                CamPostion.Z += -moveFactor * ((e.X - mouseLastX) * currentView.M31 - (e.Y - mouseLastY) * currentView.M32);

                Matrix viewMatrix = Matrix.LookAtLH(CamPostion, CamTarget, new Vector3(0, 1, 0));
                m_device.Transform.View = viewMatrix;
                mouseLastX = e.X;
                mouseLastY = e.Y;
                Render();
            }
        }
        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            float scaleFactor = -(float)e.Delta / 2000 + 1f;
            CamPostion.Subtract(CamTarget);
            CamPostion.Scale(scaleFactor);
            CamPostion.Add(CamTarget);
            Matrix viewMatrix = Matrix.LookAtLH(CamPostion, CamTarget, new Vector3(0, 1, 0));
            m_device.Transform.View = viewMatrix;
            Render();
        }
    }
}
