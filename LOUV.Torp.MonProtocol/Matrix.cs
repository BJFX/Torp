using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LOUV.Torp.MonProtocol
{
    //矩阵数据结构  
    //二维矩阵  
    public struct Matrix
    {
        public int m;
        public int n;
        public double[] arr;
    };
    public struct Input
    {
        public double x;
        public double y;
        public double z;
        public double r;
    }
    public class MatrixLocate
    {
        
        
        //矩阵方法  
        //设置mn  
        static void matrix_set(ref Matrix m, int mm, int nn)
        {
            m.m = mm;
            m.n = nn;
        }

        //设置m  
        static void matrix_set_m(ref Matrix m, int mm)
        {
            m.m = mm;
        }

        //设置n  
        static void matrix_set_n(ref Matrix m, int nn)
        {
            m.n = nn;
        }

        //初始化  
        static void matrix_init(ref Matrix m)
        {
            m.arr = new double[m.m * m.n];
        }

        //读取i,j坐标的数据  
        //失败返回-31415,成功返回值  
        static double matrix_read(Matrix m, int i, int j)
        {
            if (i >= m.m || j >= m.n)
            {
                return -31415;
            }

            return m.arr[i * m.n + j];
        }

        //写入i,j坐标的数据  
        //失败返回-1,成功返回1  
        static int matrix_write(ref Matrix m, int i, int j, double val)
        {
            if (i >= m.m || j >= m.n)
            {
                return -1;
            }


            m.arr[i * m.n + j] = val;
            return 1;
        }

        //矩阵运算  
        //成功返回1,失败返回-1  
        static int matrix_add(ref Matrix A, ref Matrix B, ref Matrix C)
        {
            int i = 0;
            int j = 0;

            //判断是否可以运算  
            if (A.m != B.m || A.n != B.n || A.m != C.m || A.n != C.n)
            {
                return -1;
            }
            //运算  
            for (i = 0; i < C.m; i++)
            {
                for (j = 0; j < C.n; j++)
                {
                    matrix_write(ref C, i, j, matrix_read(A, i, j) + matrix_read(B, i, j));
                }
            }

            return 1;
        }

        //C = A - B  
        //成功返回1,失败返回-1  
        static int matrix_subtract(ref Matrix A, ref Matrix B, ref Matrix C)
        {
            int i = 0;
            int j = 0;

            //判断是否可以运算  
            if (A.m != B.m || A.n != B.n || A.m != C.m || A.n != C.n)
            {
                return -1;
            }
            //运算  
            for (i = 0; i < C.m; i++)
            {
                for (j = 0; j < C.n; j++)
                {
                    matrix_write(ref C, i, j, matrix_read(A, i, j) - matrix_read(B, i, j));
                }
            }

            return 1;
        }

        //C = A * B  
        //成功返回1,失败返回-1  
        static int matrix_multiply(ref Matrix A, ref Matrix B, ref Matrix C)
        {
            int i = 0;
            int j = 0;
            int k = 0;
            double temp = 0;

            //判断是否可以运算  
            if (A.m != C.m || B.n != C.n || A.n != B.m)
            {
                return -1;
            }
            //运算  
            for (i = 0; i < C.m; i++)
            {
                for (j = 0; j < C.n; j++)
                {
                    temp = 0;
                    for (k = 0; k < A.n; k++)
                    {
                        temp += matrix_read(A, i, k) * matrix_read(B, k, j);
                    }
                    matrix_write(ref C, i, j, temp);
                }
            }

            return 1;
        }

        //行列式的值,只能计算2 * 2,3 * 3  
        //失败返回-31415,成功返回值  
        static double matrix_det(Matrix A)
        {
            double value = 0;

            //判断是否可以运算  
            if (A.m != A.n || (A.m != 2 && A.m != 3))
            {
                return -31415;
            }
            //运算  
            if (A.m == 2)
            {
                value = matrix_read(A, 0, 0) * matrix_read(A, 1, 1) - matrix_read(A, 0, 1) * matrix_read(A, 1, 0);
            }
            else
            {
                value = matrix_read(A, 0, 0) * matrix_read(A, 1, 1) * matrix_read(A, 2, 2) +
                        matrix_read(A, 0, 1) * matrix_read(A, 1, 2) * matrix_read(A, 2, 0) +
                        matrix_read(A, 0, 2) * matrix_read(A, 1, 0) * matrix_read(A, 2, 1) -
                        matrix_read(A, 0, 0) * matrix_read(A, 1, 2) * matrix_read(A, 2, 1) -
                        matrix_read(A, 0, 1) * matrix_read(A, 1, 0) * matrix_read(A, 2, 2) -
                        matrix_read(A, 0, 2) * matrix_read(A, 1, 1) * matrix_read(A, 2, 0);
            }

            return value;
        }

        //求转置矩阵,B = AT  
        //成功返回1,失败返回-1  
        int matrix_transpos(ref Matrix A, ref Matrix B)
        {
            int i = 0;
            int j = 0;

            //判断是否可以运算  
            if (A.m != B.n || A.n != B.m)
            {
                return -1;
            }
            //运算  
            for (i = 0; i < B.m; i++)
            {
                for (j = 0; j < B.n; j++)
                {
                    matrix_write(ref B, i, j, matrix_read(A, j, i));
                }
            }

            return 1;
        }

        //求逆矩阵,B = A^(-1)  
        //成功返回1,失败返回-1  
        int matrix_inverse(ref Matrix A, ref Matrix B)
        {
            int i = 0;
            int j = 0;
            int k = 0;
            Matrix m = new Matrix();
            double temp = 0;
            double b = 0;

            //判断是否可以运算  
            if (A.m != A.n || B.m != B.n || A.m != B.m)
            {
                return -1;
            }

            /* 
            //如果是2维或者3维求行列式判断是否可逆 
            if (A.m == 2 || A.m == 3) 
            { 
                if (det(A) == 0) 
                { 
                    return -1; 
                } 
            } 
            */

            //增广矩阵m = A | B初始化  
            matrix_set_m(ref m, A.m);
            matrix_set_n(ref m, 2 * A.m);
            matrix_init(ref m);
            for (i = 0; i < m.m; i++)
            {
                for (j = 0; j < m.n; j++)
                {
                    if (j <= A.n - 1)
                    {
                        matrix_write(ref m, i, j, matrix_read(A, i, j));
                    }
                    else
                    {
                        if (i == j - A.n)
                        {
                            matrix_write(ref m, i, j, 1);
                        }
                        else
                        {
                            matrix_write(ref m, i, j, 0);
                        }
                    }
                }
            }

            //高斯消元  
            //变换下三角  
            for (k = 0; k < m.m - 1; k++)
            {
                //如果坐标为k,k的数为0,则行变换  
                if (matrix_read(m, k, k) == 0)
                {
                    for (i = k + 1; i < m.m; i++)
                    {
                        if (matrix_read(m, i, k) != 0)
                        {
                            break;
                        }
                    }
                    if (i >= m.m)
                    {
                        return -1;
                    }
                    else
                    {
                        //交换行  
                        for (j = 0; j < m.n; j++)
                        {
                            temp = matrix_read(m, k, j);
                            matrix_write(ref m, k, j, matrix_read(m, k + 1, j));
                            matrix_write(ref m, k + 1, j, temp);
                        }
                    }
                }

                //消元  
                for (i = k + 1; i < m.m; i++)
                {
                    //获得倍数  
                    b = matrix_read(m, i, k) / matrix_read(m, k, k);
                    //行变换  
                    for (j = 0; j < m.n; j++)
                    {
                        temp = matrix_read(m, i, j) - b * matrix_read(m, k, j);
                        matrix_write(ref m, i, j, temp);
                    }
                }
            }
            //变换上三角  
            for (k = m.m - 1; k > 0; k--)
            {
                //如果坐标为k,k的数为0,则行变换  
                if (matrix_read(m, k, k) == 0)
                {
                    for (i = k + 1; i < m.m; i++)
                    {
                        if (matrix_read(m, i, k) != 0)
                        {
                            break;
                        }
                    }
                    if (i >= m.m)
                    {
                        return -1;
                    }
                    else
                    {
                        //交换行  
                        for (j = 0; j < m.n; j++)
                        {
                            temp = matrix_read(m, k, j);
                            matrix_write(ref m, k, j, matrix_read(m, k + 1, j));
                            matrix_write(ref m, k + 1, j, temp);
                        }
                    }
                }

                //消元  
                for (i = k - 1; i >= 0; i--)
                {
                    //获得倍数  
                    b = matrix_read(m, i, k) / matrix_read(m, k, k);
                    //行变换  
                    for (j = 0; j < m.n; j++)
                    {
                        temp = matrix_read(m, i, j) - b * matrix_read(m, k, j);
                        matrix_write(ref m, i, j, temp);
                    }
                }
            }
            //将左边方阵化为单位矩阵  
            for (i = 0; i < m.m; i++)
            {
                if (matrix_read(m, i, i) != 1)
                {
                    //获得倍数  
                    b = 1 / matrix_read(m, i, i);
                    //行变换  
                    for (j = 0; j < m.n; j++)
                    {
                        temp = matrix_read(m, i, j) * b;
                        matrix_write(ref m, i, j, temp);
                    }
                }
            }
            //求得逆矩阵  
            for (i = 0; i < B.m; i++)
            {
                for (j = 0; j < B.m; j++)
                {
                    matrix_write(ref B, i, j, matrix_read(m, i, j + m.m));
                }
            }

            return 1;
        }

        //矩阵拷贝:A = B  
        //成功返回1,失败返回-1  
        static int matrix_copy(ref Matrix A, ref Matrix B)
        {
            int i = 0;
            int j = 0;

            if (A.m != B.m || A.n != B.n)
            {
                return -1;
            }

            for (i = 0; i < A.m; i++)
            {
                for (j = 0; j < A.n; j++)
                {
                    matrix_write(ref B, i, j, matrix_read(A, i, j));
                }
            }

            return 1;
        }

        //求方程根,A * X = B,  
        //成功返回1,答案保存在C中,失败返回-1  
        //要求:A必须是方阵,如果A是m*m方阵,则B必须是m * 1,C必须是m * 1  
        int matrix_solve(Matrix A, Matrix B, Matrix C)
        {
            int i = 0;
            int j = 0;
            int k = 0;
            Matrix m = new Matrix();
            double temp = 0;
            double b = 0;

            //判断是否可以运算  
            if (A.m != A.n || B.n != 1 || A.m != B.m || C.n != 1 || A.m != C.m)
            {
                return -1;
            }

            /* 
            //如果是2维或者3维求行列式判断是否可逆 
            if (A.m == 2 || A.m == 3) 
            { 
                if (det(A) == 0) 
                { 
                    return -1; 
                } 
            } 
            */

            //增广矩阵m = A | B初始化  
            matrix_set_m(ref m, A.m);
            matrix_set_n(ref m, A.m + 1);
            matrix_init(ref m);
            for (i = 0; i < m.m; i++)
            {
                for (j = 0; j < m.n; j++)
                {
                    if (j <= A.n - 1)
                    {
                        matrix_write(ref m, i, j, matrix_read(A, i, j));
                    }
                    else
                    {
                        matrix_write(ref m, i, j, matrix_read(B, i, 0));
                    }
                }
            }

            //高斯消元  
            //变换下三角  
            for (k = 0; k < m.m - 1; k++)
            {
                //如果坐标为k,k的数为0,则行变换  
                if (matrix_read(m, k, k) == 0)
                {
                    for (i = k + 1; i < m.m; i++)
                    {
                        if (matrix_read(m, i, k) != 0)
                        {
                            break;
                        }
                    }
                    if (i >= m.m)
                    {
                        return -1;
                    }
                    else
                    {
                        //交换行  
                        for (j = 0; j < m.n; j++)
                        {
                            temp = matrix_read(m, k, j);
                            matrix_write(ref m, k, j, matrix_read(m, k + 1, j));
                            matrix_write(ref m, k + 1, j, temp);
                        }
                    }
                }

                //消元  
                for (i = k + 1; i < m.m; i++)
                {
                    //获得倍数  
                    b = matrix_read(m, i, k) / matrix_read(m, k, k);
                    //行变换  
                    for (j = 0; j < m.n; j++)
                    {
                        temp = matrix_read(m, i, j) - b * matrix_read(m, k, j);
                        matrix_write(ref m, i, j, temp);
                    }
                }
            }
            //变换上三角  
            for (k = m.m - 1; k > 0; k--)
            {
                //如果坐标为k,k的数为0,则行变换  
                if (matrix_read(m, k, k) == 0)
                {
                    for (i = k + 1; i < m.m; i++)
                    {
                        if (matrix_read(m, i, k) != 0)
                        {
                            break;
                        }
                    }
                    if (i >= m.m)
                    {
                        return -1;
                    }
                    else
                    {
                        //交换行  
                        for (j = 0; j < m.n; j++)
                        {
                            temp = matrix_read(m, k, j);
                            matrix_write(ref m, k, j, matrix_read(m, k + 1, j));
                            matrix_write(ref m, k + 1, j, temp);
                        }
                    }
                }

                //消元  
                for (i = k - 1; i >= 0; i--)
                {
                    //获得倍数  
                    b = matrix_read(m, i, k) / matrix_read(m, k, k);
                    //行变换  
                    for (j = 0; j < m.n; j++)
                    {
                        temp = matrix_read(m, i, j) - b * matrix_read(m, k, j);
                        matrix_write(ref m, i, j, temp);
                    }
                }
            }
            //将左边方阵化为单位矩阵  
            for (i = 0; i < m.m; i++)
            {
                if (matrix_read(m, i, i) != 1)
                {
                    //获得倍数  
                    b = 1 / matrix_read(m, i, i);
                    //行变换  
                    for (j = 0; j < m.n; j++)
                    {
                        temp = matrix_read(m, i, j) * b;
                        matrix_write(ref m, i, j, temp);
                    }
                }
            }
            //求得解  
            for (i = 0; i < C.m; i++)
            {
                matrix_write(ref C, i, 0, matrix_read(m, i, m.n - 1));
            }


            return 1;
        }

        //利用克莱姆法则求方程根,A * X = B,  
        //成功返回1,答案保存在C中,失败返回-1  
        //要求:A必须是方阵,如果A是m*m方阵,则B必须是m * 1,C必须是m * 1  
        static int matrix_det_solve(Matrix A, Matrix B, Matrix C)
        {
            Matrix m = new Matrix();
            double det_m;
            double det_m_temp;
            int i = 0;
            int j = 0;

            //初始化m  
            matrix_set_m(ref m, A.m);
            matrix_set_n(ref m, A.n);
            matrix_init(ref m);

            //得到A的行列式值  
            det_m = matrix_det(A);
            //判断是否有效  
            if (det_m == 0)
            {

                return -1;
            }

            for (i = 0; i < 2; i++)
            {
                //得到新的行列式  
                matrix_copy(ref A, ref m);
                for (j = 0; j < 2; j++)
                {
                    matrix_write(ref m, j, i, matrix_read(B, j, 0));
                }
                det_m_temp = matrix_det(m);

                //求解  
                matrix_write(ref C, i, 0, det_m_temp / det_m);
            }


            return 1;
        }

        //定位函数  
        //PI:输入3点坐标,格式:是3 * 2维  
        //D:3点距离未知点距离数组  
        //PO:输出坐标  
        //成功返回1,失败返回-1  
        public static int locate(Matrix PI, double[] D, out double x, out double y)
        {
            int i = 0;
            int j = 0;
            Matrix A = new Matrix();
            Matrix B = new Matrix();
            Matrix C = new Matrix();
            double temp = 0;

            //判断是否可以运算  
            if (PI.m != 3 || PI.n != 2)
            {
                x = 0;
                y = 0;
                return -1;
            }

            //初始化ABC矩阵  
            matrix_set_m(ref A, 2);
            matrix_set_n(ref A, 2);
            matrix_init(ref A);
            matrix_set_m(ref B, 2);
            matrix_set_n(ref B, 1);
            matrix_init(ref B);
            matrix_set_m(ref C, 2);
            matrix_set_n(ref C, 1);
            matrix_init(ref C);

            //初始化A矩阵  
            for (i = 0; i < 2; i++)
            {
                for (j = 0; j < 2; j++)
                {
                    temp = matrix_read(PI, i + 1, j) - matrix_read(PI, i, j);
                    matrix_write(ref A, i, j, temp);
                }
            }

            //初始化B矩阵  
            for (i = 0; i < 2; i++)
            {
                temp = matrix_read(PI, i + 1, 0) * matrix_read(PI, i + 1, 0);
                temp += matrix_read(PI, i + 1, 1) * matrix_read(PI, i + 1, 1);
                temp -= matrix_read(PI, i, 0) * matrix_read(PI, i, 0);
                temp -= matrix_read(PI, i, 1) * matrix_read(PI, i, 1);
                temp -= D[i + 1] * D[i + 1] - D[i] * D[i];
                temp /= 2;
                matrix_write(ref B, i, 0, temp);
            }

            //解方程  
            //if (matrix_solve(&A,&B,&C) > 0)  
            if (matrix_det_solve(A, B, C) > 0)
            {
                x = matrix_read(C, 0, 0);
                y = matrix_read(C, 1, 0);

                return 1;
            }
            x = 0;
            y = 0;
            return -1;
        }
        public static void InitMatrix(ref Matrix m,Input i1, Input i2, Input i3,ref double[] D)
        {
            matrix_set(ref m, 3, 2);
            matrix_init(ref m);
            //获得坐标  
            matrix_write(ref m, 0, 0, i1.x);
            matrix_write(ref m, 0, 1, i1.y);
            
            D[0] = Math.Sqrt(i1.r * i1.r - i1.z * i1.z);

            matrix_write(ref m, 1, 0, i2.x);
            matrix_write(ref m, 1, 1, i2.y);

            D[1] = Math.Sqrt(i2.r * i2.r - i2.z * i2.z);

            matrix_write(ref m, 2, 0, i3.x);
            matrix_write(ref m, 2, 1, i3.y);

            D[2] = Math.Sqrt(i3.r * i3.r - i3.z * i3.z);
        }

    }
}
