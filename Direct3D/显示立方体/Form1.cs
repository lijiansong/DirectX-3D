﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace 显示立方体
{
    public partial class Form1 : Form
    {
        private Device device = null;
        bool pause = false;
        VertexBuffer vertexBuffer = null;
        float Angle = 0, ViewZ = -6.0f;

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
            Device dev = (Device)sender;		//阴影部分是所作修改，正方形有6个顶点
            vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionColored), 6,
            dev, 0, CustomVertex.TransformedColored.Format, Pool.Default);
            vertexBuffer.Created += new System.EventHandler(this.OnCreateVertexBuffer);
            this.OnCreateVertexBuffer(vertexBuffer, null);

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
            if (device == null) 						//如果未建立设备对象，退出
                return;
            if (pause)
                return;
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.LightBlue, 1.0f, 0);
            device.BeginScene();						//开始渲染
            SetupMatrices();
            device.SetStreamSource(0, vertexBuffer, 0);
            device.VertexFormat = CustomVertex.PositionColored.Format;
            device.Transform.World = Matrix.Translation(0, 0, -1);//沿Z轴向观察者方向移动1个单位
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);    //绘制正前面
            //旋转180度是为了从外侧看，按顺时针方向绘制三角形，因背面剔除打开，内侧不被看到
            device.Transform.World = Matrix.RotationY((float)Math.PI) * Matrix.Translation(0, 0, 1);
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);    //绘制正后面
            device.Transform.World =
            Matrix.RotationY(-(float)Math.PI / 2) * Matrix.Translation(1, 0, 0);
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);		//绘制右侧面
            device.Transform.World =
        Matrix.RotationY((float)Math.PI / 2) * Matrix.Translation(-1, 0, 0);
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);		//绘制左侧面
            device.Transform.World =
        Matrix.RotationX((float)Math.PI / 2) * Matrix.Translation(0, 1, 0);
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);		//绘制下面
            device.Transform.World =
        Matrix.RotationX(-(float)Math.PI / 2) * Matrix.Translation(0, -1, 0);
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);   	//绘制上面
            device.EndScene();										//渲染结束
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
            CustomVertex.PositionColored[] verts =
                    (CustomVertex.PositionColored[])vertexBuffer.Lock(0, 0);
            verts[0].Position = new Vector3(-1.0f, -1.0f, 0.0f);  //顶点0位置，注意为Vector3
            verts[0].Color = System.Drawing.Color.Aqua.ToArgb();    	  //顶点0颜色
            verts[1].Position = new Vector3(1.0f, 1.0f, 0.0f);	  	  //顶点1位置
            verts[1].Color = System.Drawing.Color.Brown.ToArgb();
            verts[2].Position = new Vector3(1.0f, -1.0f, 0.0f);	      //顶点2位置
            verts[2].Color = System.Drawing.Color.LightPink.ToArgb();
            verts[3].Position = new Vector3(-1.0f, -1.0f, 0.0f);	  //顶点3位置
            verts[3].Color = System.Drawing.Color.Aqua.ToArgb();    	  //顶点3颜色
            verts[4].Position = new Vector3(-1.0f, 1.0f, 0.0f);	  	  //顶点4位置
            verts[4].Color = System.Drawing.Color.Red.ToArgb();
            verts[5].Position = new Vector3(1.0f, 1.0f, 0.0f);	      //顶点5位置
            verts[5].Color = System.Drawing.Color.Brown.ToArgb();
            vertexBuffer.Unlock();

        }

        private void SetupMatrices()		//修改Device的3个变换
        {
            device.Transform.World = Matrix.RotationY(Angle);	//世界变换矩阵，沿Y轴旋转
            device.Transform.View = Matrix.LookAtLH(new Vector3(0.0f, 3.0f, ViewZ),//观察变换矩阵
                        new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));
            device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4,
                        1.0f, 1.0f, 100.0f);		//投影变换语句仍可以放到OnResetDevice方法中
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

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeGraphics();
            this.Show();
            Render();
        }

    }
}
