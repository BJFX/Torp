using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LOUV.Torp.Monitor.Core
{
    public class SettleSoundFile
    {
        public int maxDepth = 13000;
        public int[] SVPd = new int[13000 + 31];
        public double[] SVPc = new double[13000 + 31];

        public SettleSoundFile(string FileName)
        {
            string[] txt = File.ReadAllLines(FileName);
            double[] DepthVector = new double[txt.Length];
            double[] VeloVctor = new double[txt.Length];
            double[] NegativeDepthVector = new double[txt.Length];

            double temp = 0.0;
            int index = 0;
            //将数据转化成浮点数,并找到最大值及其地址
            for (int i = 0; i < txt.Length; i++)
            {
                string[] string_row = txt[i].Split(' ');
                DepthVector[i] = Convert.ToDouble(string_row[0]);
                VeloVctor[i] = Convert.ToDouble(string_row[1]);
                NegativeDepthVector[i] = -DepthVector[i];
                if (temp < Math.Abs(DepthVector[i]))
                {
                    temp = Math.Abs(DepthVector[i]);
                    index = i;
                }
            }
            if (!(temp == DepthVector[index]))
            {
                DepthVector = NegativeDepthVector;
            }

            double[] DepthVector2 = new double[index + 1];
            double[] VeloVctor2 = new double[index + 1];
            Array.Copy(DepthVector, 0, DepthVector2, 0, index + 1);
            Array.Copy(VeloVctor, 0, VeloVctor2, 0, index + 1);
            Array.Sort(DepthVector2, VeloVctor2);

            //从开始下水至最大深度，对其速度进行排序并删除重复的深度值
            int count = 1;
            for (int k = 1; k < index + 1; k++)
            {
                if (!(DepthVector2[k - 1] == DepthVector2[k]))
                {
                    count = count + 1;
                }
            }

            double[] DepthVector3 = new double[count];
            double[] VeloVctor3 = new double[count];

            for (int k = 0; k < index + 1; k++)
            {
                if (k == 0)
                {
                    count = 1;
                    DepthVector3[0] = DepthVector2[k];
                    VeloVctor3[0] = VeloVctor2[k];
                }
                else
                    if (!(DepthVector2[k - 1] == DepthVector2[k]))
                    {
                        count = count + 1;
                        DepthVector3[count - 1] = DepthVector2[k];
                        VeloVctor3[count - 1] = VeloVctor2[k];
                    }
            }
            if (count < 11)
            {
                throw new Exception("声速剖面数据过少或有问题!");
            }

            double[] DepthVector4 = new double[11];
            double[] VeloVctor4 = new double[11];
            Array.Copy(DepthVector3, count - 11, DepthVector4, 0, 11);
            Array.Copy(VeloVctor3, count - 11, VeloVctor4, 0, 11);

            //线性拟合
            double[] coeffV = MultiLine(DepthVector4, VeloVctor4, 11, 1);
            double arrX = DepthVector3[count - 1] + 1;
            int m = 0;
            m = (int)Math.Ceiling(100000 - (DepthVector3[count - 1] + 1));
            double[] AmedD = new double[m];
            double[] AmedV = new double[m];
            for (int n = 0; n < m; n++)
            {
                if (n == 0)
                    AmedD[n] = DepthVector3[count - 1] + 1;
                else
                    AmedD[n] = AmedD[n - 1] + 1;
                AmedV[n] = AmedD[n] * coeffV[1] + coeffV[0];
            }
            double[] DepthVector5 = new double[m + count];
            double[] VeloVctor5 = new double[m + count];
            Array.Copy(DepthVector3, 0, DepthVector5, 0, count);
            Array.Copy(AmedD, 0, DepthVector5, count, m);
            Array.Copy(VeloVctor3, 0, VeloVctor5, 0, count);
            Array.Copy(AmedV, 0, VeloVctor5, count, m);

            //考虑深沉为正的情况
            DepthVector5[0] = -30;

            int Serch_start = 0;
            for (int d = 0; d < maxDepth + 31; d++)
            {
                //给SVPd赋值
                if (d == 0)
                {
                    SVPd[d] = -30;
                }
                else
                {
                    SVPd[d] = SVPd[d - 1] + 1;
                }

                //线性插值，起始位置从上次插值位置开始寻找
                for (int h = Serch_start; h < m + count; h++)
                {
                    if (SVPd[d] <= DepthVector5[h])
                    {
                        if (DepthVector5[h] == SVPd[d])
                        {
                            SVPc[d] = VeloVctor5[h];
                        }
                        else
                        {
                            SVPc[d] = (VeloVctor5[h] - VeloVctor5[h - 1]) / (DepthVector5[h] - DepthVector5[h - 1]) * (SVPd[d] - DepthVector5[h - 1]) + VeloVctor5[h - 1];
                        }
                        Serch_start = h;
                        break;
                    }
                }
            }
        }



        public static double[] MultiLine(double[] arrX, double[] arrY, int length, int dimension)//二元多次线性方程拟合曲线
        {
            int n = dimension + 1;                  //dimension次方程需要求 dimension+1个 系数
            double[,] Guass = new double[n, n + 1];      //高斯矩阵 例如：y=a0+a1*x+a2*x*x
            for (int i = 0; i < n; i++)
            {
                int j;
                for (j = 0; j < n; j++)
                {
                    Guass[i, j] = SumArr(arrX, j + i, length);
                }
                Guass[i, j] = SumArr(arrX, i, arrY, 1, length);
            }
            return ComputGauss(Guass, n);
        }

        public static double SumArr(double[] arr, int n, int length) //求数组的元素的n次方的和
        {
            double s = 0;
            for (int i = 0; i < length; i++)
            {
                if (arr[i] != 0 || n != 0)
                    s = s + Math.Pow(arr[i], n);
                else
                    s = s + 1;
            }
            return s;
        }

        public static double SumArr(double[] arr1, int n1, double[] arr2, int n2, int length)//求数组1中元素的n1次方与数组2中元素的n2次方相乘的和
        {
            double s = 0;
            for (int i = 0; i < length; i++)
            {
                if ((arr1[i] != 0 || n1 != 0) && (arr2[i] != 0 || n2 != 0))
                    s = s + Math.Pow(arr1[i], n1) * Math.Pow(arr2[i], n2);
                else
                    s = s + 1;
            }
            return s;

        }

        public static double[] ComputGauss(double[,] Guass, int n)
        {
            int i, j;
            int k, m;
            double temp;
            double max;
            double s;
            double[] x = new double[n];
            for (i = 0; i < n; i++) x[i] = 0.0;//初始化

            for (j = 0; j < n; j++)//将矩阵化成阶梯型
            {
                max = 0;
                k = j;
                for (i = j; i < n; i++)//选择主元
                {
                    if (Math.Abs(Guass[i, j]) > max)
                    {
                        max = Guass[i, j];
                        k = i;
                    }
                }


                if (k != j)//找到主元换行
                {
                    for (m = j; m < n + 1; m++)
                    {
                        temp = Guass[j, m];
                        Guass[j, m] = Guass[k, m];
                        Guass[k, m] = temp;
                    }
                }
                if (0 == max)
                {
                    // "此线性方程为奇异线性方程" 
                    return x;
                }

                for (i = j + 1; i < n; i++)//进行行变换
                {
                    s = Guass[i, j];
                    for (m = j; m < n + 1; m++)
                    {
                        Guass[i, m] = Guass[i, m] - Guass[j, m] * s / (Guass[j, j]);
                    }
                }

            }//结束for (j=0;j<n;j++)

            for (i = n - 1; i >= 0; i--)//逐个求解未知数
            {
                s = 0;
                for (j = i + 1; j < n; j++)
                {
                    s = s + Guass[i, j] * x[j];
                }
                x[i] = (Guass[i, n] - s) / Guass[i, i];
            }
            return x;
        }//返回值是函数的系数

    }
}
