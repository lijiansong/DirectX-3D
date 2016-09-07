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

namespace 复杂变换
{
    public partial class Form1 : Form
    {
        private Device device = null;
        bool pause = false;
        VertexBuffer vertexBuffer = null;
        float Angle = 0, ViewZ = -6.0f;
        IndexBuffer indexBuffer = null;

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
                device = new Device(0, DeviceType.Hardware, this, 	 //建立设备类对象
          CreateFlags.SoftwareVertexProcessing, presentParams);
                //设置设备重置事件(device.DeviceReset)事件函数为this.OnResetDevice
                device.DeviceReset += new System.EventHandler(this.OnResetDevice);
                this.OnCreateDevice(device, null);//自定义方法，初始化Device的工作放到这个方法中
                this.OnResetDevice(device, null);//调用设备重置事件(device.DeviceReset)事件函数
            }		//设备重置事件函数要设置Device参数，初始函数中必须调用该函数
            catch (DirectXException)
            {
                return false;
            }
            return true;
        }

        public void OnCreateDevice(object sender, EventArgs e)
        {
            Device dev = (Device)sender;		//阴影部分是所作修改，正方体有8个顶点
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), 8,
            dev, 0, CustomVertex.TransformedColored.Format, Pool.Default);
            indexBuffer = new IndexBuffer(typeof(int), 36, dev, 0, Pool.Default);  //顶点索引
            vertexBuffer.Created += new System.EventHandler(this.OnCreateVertexBuffer);
            indexBuffer.Created += new EventHandler(indexBuffer_Created);
            this.OnCreateVertexBuffer(vertexBuffer, null);
            this.indexBuffer_Created(indexBuffer, null);


        }

        public void OnResetDevice(object sender, EventArgs e)
        {
            Device dev = (Device)sender;
            //背面剔除方式为只显示顺时针三角形，因为正方体应该只看到外表面
            dev.RenderState.CullMode = Cull.CounterClockwise;
            dev.RenderState.Lighting = false;				//取消灯光

        }

        public void Render()		//渲染方法，本方法没有任何渲染代码，可认为是渲染方法的框架
        {
            if (device == null) 	//如果未建立设备对象，退出
                return;
            if (pause)
                return;
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.WhiteSmoke, 1.0f, 0);
            device.BeginScene();			//开始渲染
            SetupMatrices();
            device.SetStreamSource(0, vertexBuffer, 0);
            device.VertexFormat = CustomVertex.PositionColored.Format;
            device.Indices = indexBuffer;
            int iTime = Environment.TickCount % 10000;
            float Angle = iTime * (2.0f * (float)Math.PI) / 10000.0f;
            device.Transform.World = Matrix.RotationY(Angle);	   //第1个正方体
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 8, 0, 12);
            device.Transform.World = Matrix.RotationY(Angle) * 	   //第2个正方体自身转动
        Matrix.Translation(2.0f, 0.0f, 0.0f) *  //第2个正方体移动位置
        Matrix.RotationY(Angle);  //在移动后位置围绕y轴转动
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 8, 0, 12);
            device.EndScene();								   //渲染结束
            device.Present();	//更新显示区域，把后备缓存的D图形送到图形卡的显存中显示
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            this.Render();
        }
        private void Form1_Resize(object sender, EventArgs e)
        {
            pause = ((this.WindowState == FormWindowState.Minimized) || !this.Visible);
        }
        public void OnCreateVertexBuffer(object sender, EventArgs e)
        {
            CustomVertex.PositionColored[] verts =			//这里仅仅将正方体的尺寸改小
(CustomVertex.PositionColored[])vertexBuffer.Lock(0, 0);
            verts[0].Position = new Vector3(-0.2f, 0.2f, 0.2f);	  //顶点0位置，注意为Vector3
            verts[0].Color = System.Drawing.Color.Aqua.ToArgb();    	  		//顶点0颜色
            verts[1].Position = new Vector3(0.2f, 0.2f, 0.2f);	  	  		//顶点1位置
            verts[1].Color = System.Drawing.Color.Brown.ToArgb();
            verts[2].Position = new Vector3(-0.2f, -0.2f, 0.2f);	      	//顶点2位置
            verts[2].Color = System.Drawing.Color.LightPink.ToArgb();
            verts[3].Position = new Vector3(0.2f, -0.2f, 0.2f);	  			//顶点3位置
            verts[3].Color = System.Drawing.Color.Red.ToArgb();    	  		//顶点3颜色
            verts[4].Position = new Vector3(-0.2f, 0.2f, -0.2f);	  	  	//顶点4位置
            verts[4].Color = System.Drawing.Color.Green.ToArgb();
            verts[5].Position = new Vector3(0.2f, 0.2f, -0.2f);	      		//顶点5位置
            verts[5].Color = System.Drawing.Color.Black.ToArgb();
            verts[6].Position = new Vector3(-0.2f, -0.2f, -0.2f);	  	  	//顶点6位置
            verts[6].Color = System.Drawing.Color.LightPink.ToArgb();
            verts[7].Position = new Vector3(0.2f, -0.2f, -0.2f);	      	//顶点7位置
            verts[7].Color = System.Drawing.Color.Red.ToArgb();
            vertexBuffer.Unlock();
        }

        private void SetupMatrices()		//修改Device的3个变换
        {
            device.Transform.View = Matrix.LookAtLH(new Vector3(0.0f, 3.0f, -5.0f),
                 new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));
            device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4,
                        1.0f, 1.0f, 100.0f);

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)				//e.KeyCode是键盘每个键的编号
            {
                case Keys.Left:			//Keys.Left是左箭头键编号，三角形沿Y轴左转
                    Angle += 0.1F;
                    break;
                case Keys.Right:			//三角形沿Y轴右转
                    Angle -= 0.1F;
                    break;
                case Keys.Down:			//三角形离观察者越来越远
                    ViewZ += 0.1F;
                    break;
                case Keys.Up:				//三角形离观察者越来越近
                    ViewZ -= 0.1F;
                    break;
            }

        }

        void indexBuffer_Created(object sender, EventArgs e)
        {    //下面数组每3个数表示一个三角形的索引，每2个三角形绘制1个面，
            int[] index =		//按顺序分别绘制前面、右面、上面、左面、后面和下面
            { 4, 5, 6, 5, 7, 6, 5, 1, 7, 7, 1, 3, 4, 0, 1, 4, 1, 5, 2, 0, 4, 2, 4, 6, 3, 1, 0, 3, 0, 2, 2, 6, 7, 2, 7, 3 };
            int[] indexV = (int[])indexBuffer.Lock(0, 0);
            for (int i = 0; i < 36; i++)
            {
                indexV[i] = index[i];
            }
            indexBuffer.Unlock();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeGraphics();
            this.Show();
            Render();
        }

    }
}
