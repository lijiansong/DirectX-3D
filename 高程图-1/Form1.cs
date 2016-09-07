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

namespace 高程图
{
    public partial class Form1 : Form
    {
        private Device m_device = null;
        bool pause = false;

        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private int[] indices;
        
        private void SetupMatrices()
        {
            m_device.Transform.World = Matrix.RotationY(0);//世界变换
            m_device.Transform.View = Matrix.LookAtLH(//取景变换
                new Vector3(0.0f,5.0f,-25.0f),
                new Vector3(0.0f,0.0f,0.0f),
                new Vector3(0.0f,1.0f,0.0f));
            m_device.Transform.Projection = Matrix.PerspectiveFovLH(//投影变换
                (float)Math.PI/4,
                (float)this.Width/(float)this.Height,
                1.0f,100.0f);
        }

        public Form1()
        {
            InitializeComponent();
        }

        public bool InitializeGraphics()
        {
            try
            {
                PresentParameters presentParams = new PresentParameters();
                presentParams.Windowed = true;				//不是全屏显示，在一个窗口显示
                presentParams.SwapEffect = SwapEffect.Discard;		 //后备缓存交换的方式
                presentParams.EnableAutoDepthStencil = true;			 //允许使用自动深度模板测试
                //深度缓冲区单元为16位二进制数
                presentParams.AutoDepthStencilFormat = DepthFormat.D16;
                m_device = new Device(0, DeviceType.Hardware, this, 	 //建立设备类对象
          CreateFlags.SoftwareVertexProcessing, presentParams);
                //设置设备重置事件(device.DeviceReset)事件函数为this.OnResetDevice
                m_device.DeviceReset += new System.EventHandler(this.OnResetDevice);
                this.OnCreateDevice(m_device, null);//自定义方法，初始化Device的工作放到这个方法中
                this.OnResetDevice(m_device, null);//调用设备重置事件(device.DeviceReset)事件函数
            }		//设备重置事件函数要设置Device参数，初始函数中必须调用该函数
            catch (DirectXException)
            {
                return false;
            }
            return true;
        }

        public void OnCreateDevice(object sender, EventArgs e)
        {
            Device dev = (Device)sender;
            //申请建模使用的空间
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored),
                5, m_device, Usage.Dynamic | Usage.WriteOnly,
                CustomVertex.PositionColored.Format,
                Pool.Default);
            vertexBuffer.Created += new System.EventHandler(this.OnCreateVertexBuffer);
            this.OnCreateVertexBuffer(m_device, null);
            //申请建模使用的空间
            indexBuffer = new IndexBuffer(typeof(int),
                6, m_device, Usage.WriteOnly, Pool.Default);
            indexBuffer.Created += new System.EventHandler(this.OnCreateIndices);
            this.OnCreateIndices(m_device,null);
        }
        private void OnCreateVertexBuffer(object sender, EventArgs e)//定义顶点
        {
            CustomVertex.PositionColored[] vertices = new CustomVertex.PositionColored[5];
            vertices[0].Position = new Vector3(0.0f, 0.0f, 0.0f);
            vertices[0].Color = Color.White.ToArgb();
            vertices[1].Position = new Vector3(5f, 0f, 0f);
            vertices[1].Color = Color.White.ToArgb();
            vertices[2].Position = new Vector3(10f, 0f, 0f);
            vertices[2].Color = Color.White.ToArgb();
            vertices[3].Position = new Vector3(5f, 5f, 0f);
            vertices[3].Color = Color.White.ToArgb();
            vertices[4].Position = new Vector3(10f, 5f, 0f);
            vertices[4].Color = Color.White.ToArgb();
            vertexBuffer.SetData(vertices, 0, LockFlags.None);//表示将顶点缓冲对象链接到顶点坐标上
        }

        private void OnCreateIndices(object sender, EventArgs e)//定义索引
        {
            indices = new int[6];
            indices[0] = 3;
            indices[1] = 1;
            indices[2] = 0;
            indices[3] = 4;
            indices[4] = 2;
            indices[5] = 1;
            indexBuffer.SetData(indices, 0, LockFlags.None);
        }
        public void OnResetDevice(object sender, EventArgs e)
        {
            Device dev = (Device)sender;
            dev.RenderState.CullMode = Cull.None;
            dev.RenderState.Lighting = false;
            SetupMatrices();
        }

        public void Render()		//渲染方法，本方法没有任何渲染代码，可认为是渲染方法的框架
        {
            if (m_device == null) 	//如果未建立设备对象，退出
                return;
            if (pause)
                return;

            //下边函数将显示区域初始化为蓝色，第1个参数指定要初始化目标窗口包括深度缓冲区
            //第2个参数是我们所要填充的颜色。第3、第4个参数一般为1.0f, 0。
            m_device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, System.Drawing.Color.Blue, 1.0f, 0);
            m_device.BeginScene();	//开始渲染
            //渲染代码必须放在device.BeginScene()和device.Present()之间
            //m_device.RenderState.CullMode = Cull.None;//取消背面删除
            m_device.VertexFormat = CustomVertex.PositionColored.Format;
            m_device.SetStreamSource(0,vertexBuffer,0);
            m_device.Indices = indexBuffer;
            m_device.DrawIndexedPrimitives(PrimitiveType.TriangleList,0,0,5,0,2);
            m_device.EndScene();		//渲染结束
            m_device.Present();		//更新显示区域，把后备缓存的3D图形送到屏幕显示区中显示
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            this.Render();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            pause = ((this.WindowState == FormWindowState.Minimized) || !this.Visible);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Show();
            InitializeGraphics();
            Render();
        }
    }
}
