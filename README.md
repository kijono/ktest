# ktest
Unity Test Examples

### 2. computeShader 计算点坐标（Unity 2018.4.30f）
使用cs计算一组mesh位置，并输出到指定buffer中，  
最后调用 Graphics.DrawMeshInstancedIndirect 渲染

![csmove](https://user-images.githubusercontent.com/4172198/158984295-e370cfab-2804-43d3-9ecf-42e885710419.jpg)


### 2. computeShader 处理mat运算（Unity 2018.4.30f）
使用cs渲染一组分形方块，在gpu中计算分形元素的旋转,  
仅在初始化时提交一次matbuffer以及旋转矩阵等数据  
调用Graphics.DrawMeshInstancedIndirect 接口绘制

![csmat](https://user-images.githubusercontent.com/4172198/158965562-642019f2-00a2-447e-8bbb-82fe7ae9e13a.jpg)
