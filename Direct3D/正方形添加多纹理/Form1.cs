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

namespace 正方形添加多纹理
{
    public partial class Form1 : Form
    {
        private Device device = null;
        bool pause = false;
        VertexBuffer vertexBuffer1 = null;
        Texture texture = null;
        Texture texture1 = null;
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
            Device dev = (Device)sender;   	//注意有阴影部分
            vertexBuffer1 = new VertexBuffer(typeof(CustomVertex.PositionTextured), 6, dev, 0,
         CustomVertex.PositionTextured.Format, Pool.Default);
            vertexBuffer1.Created += new EventHandler(vertexBuffer1_Created);
            this.vertexBuffer1_Created(vertexBuffer1, null);
            texture = TextureLoader.FromFile(dev, Application.StartupPath + @"\..\..\..\p1.bmp");
            texture1 = TextureLoader.FromFile(dev, Application.StartupPath + @"\..\..\..\p2.bmp");

        }

        public void OnResetDevice(object sender, EventArgs e)
        {
            Device dev = (Device)sender;
            dev.RenderState.CullMode = Cull.None;		//取消背面剔除
            dev.RenderState.Lighting = false;			//取消灯光
            SetupMatrices();		//在程序运行期间，Device的3个变换不改变，因此放在此处
            device.SamplerState[0].MagFilter = TextureFilter.Linear;	//使用线性滤波器
        }

        public void Render()		//渲染方法，本方法没有任何渲染代码，可认为是渲染方法的框架
        {
            if (device == null) 		//如果未建立设备对象，退出
                return;
            if (pause)
                return;
            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Blue, 1.0f, 0);
            device.BeginScene();				//开始渲染	
            device.SetTexture(0, texture);		//索引号为0的纹理是墙壁图案
            device.SetTexture(1, texture1);		//索引号为1的纹理是光影图案
            device.TextureState[0].TextureCoordinateIndex = 0;	//纹理坐标Tu和Tv初始值
            device.TextureState[1].TextureCoordinateIndex = 0;
            device.SamplerState[0].MagFilter = TextureFilter.Linear;  //放大图形使用线形滤波器
            device.SamplerState[1].MagFilter = TextureFilter.Linear;  //缩小图形使用线形滤波器
            device.TextureState[0].ColorOperation = TextureOperation.SelectArg1;
            device.TextureState[0].ColorArgument1 = TextureArgument.TextureColor;
            //以下两句设置纹理0为半透明的，和纹理1混合后能看到混合效果，透明效果见10.2节
            device.TextureState[0].AlphaOperation = TextureOperation.SelectArg1;
            device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;
            device.TextureState[1].ColorOperation = TextureOperation.Modulate;
            device.TextureState[1].ColorArgument1 = TextureArgument.TextureColor;
            device.TextureState[1].ColorArgument2 = TextureArgument.Current;
            //device.TextureState[1].AlphaOperation = TextureOperation.Disable;
            //device.TextureState[2].ColorOperation = TextureOperation.Disable;
            //device.TextureState[2].AlphaOperation = TextureOperation.Disable;
            device.SetStreamSource(0, vertexBuffer1, 0);
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            device.EndScene();		//渲染结束
            device.Present();		//更新显示区域，把后备缓存的图形送到图形卡的显存中显示

        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            this.Render();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            pause = ((this.WindowState == FormWindowState.Minimized) || !this.Visible);
        }
        void vertexBuffer1_Created(object sender, EventArgs e)
        {
            CustomVertex.PositionTextured[] verts =
            (CustomVertex.PositionTextured[])vertexBuffer1.Lock(0, 0);		//墙壁
            verts[0].Position = new Vector3(-2.0f, -2.0f, 2.0f);	  	//顶点0位置
            verts[0].Tu = 0.0f;    	  //顶点0纹理坐标Tu
            verts[0].Tv = 1.0f;
            verts[1].Position = new Vector3(-2.0f, 2.0f, 2.0f);	  	  		//顶点1位置
            verts[1].Tu = 0.0f;    	  //顶点1纹理坐标Tu
            verts[1].Tv = 0.0f;
            verts[2].Position = new Vector3(2.0f, 2.0f, 2.0f);	      		//顶点2位置
            verts[2].Tu = 1.0f;    	  //顶点2纹理坐标Tu
            verts[2].Tv = 0.0f;
            verts[3].Position = new Vector3(-2.0f, -2.0f, 2.0f);	  		//顶点3位置
            verts[3].Tu = 0.0f;    	  //顶点3纹理坐标Tu
            verts[3].Tv = 1.0f;
            verts[4].Position = new Vector3(2.0f, 2.0f, 2.0f);	  	  		//顶点4位置
            verts[4].Tu = 1.0f;    	  //顶点4纹理坐标Tu
            verts[4].Tv = 0.0f;
            verts[5].Position = new Vector3(2.0f, -2.0f, 2.0f);	      		//顶点5位置
            verts[5].Tu = 1.0f;    	  //顶点5纹理坐标Tu
            verts[5].Tv = 1.0f;
            vertexBuffer1.Unlock();
        }

        private void SetupMatrices()		//修改Device的3个变换
        {
            device.Transform.World = Matrix.RotationY(0);	//世界变换矩阵
            device.Transform.View = Matrix.LookAtLH(new Vector3(0.0f, 0.0f, -4.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));
            device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, 1.0f, 1.0f, 100.0f);	//投影变换矩阵
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeGraphics();
            this.Show();
            Render();
        }
    }
}
