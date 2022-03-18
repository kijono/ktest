using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CSMoveExample : MonoBehaviour
{
    [SerializeField, Range(10, 1000)]
    public int resolution = 200;
    ComputeBuffer argsBuffer;
    ComputeBuffer positionsBuffer;

    public enum KernelFunction{
        Wave, MultiWave, Ripple, Sphere, Torus
    }

    public KernelFunction kernelFunction = KernelFunction.Wave;
    public KernelFunction transitionFunction = KernelFunction.Wave;
    
    [SerializeField]
    ComputeShader computeShader = default;
    static readonly int
        positionsId = Shader.PropertyToID("_Positions"),
        resolutionId = Shader.PropertyToID("_Resolution"),
        scaleId = Shader.PropertyToID("_Scale"),
        stepId = Shader.PropertyToID("_Step"),
        timeId = Shader.PropertyToID("_Time"),
        progressId = Shader.PropertyToID("_Progress");
    
    [SerializeField]
    Material material = default;

    [SerializeField]
    Mesh mesh = default;

    private uint[] args = new uint[]{ 0, 0, 0, 0, 0 };
    private void OnEnable()
    {
        positionsBuffer = new ComputeBuffer(resolution * resolution, 3 * 4);
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        args[0] = (mesh != null) ? (uint)mesh.GetIndexCount(0) : 0;
        args[1] = (uint)(resolution * resolution);
        argsBuffer.SetData(args);

        transitioning = false;
    }

    private void OnDisable()
    {
        positionsBuffer.Release();
        positionsBuffer = null;

        argsBuffer.Release();
        argsBuffer = null;
    }

    bool transitioning = false;
    float duration = 0f;
    public float transitionDuration = 1f;

    KernelFunction curFunction = default;
    void UpdateFunctionOnGPU()
    {
        if(material == null || positionsBuffer == null)
            return;
        
        //if count change, re init info
        if(positionsBuffer.count != resolution * resolution){
            OnDisable();
            OnEnable();
        }
        
        if(!transitioning && transitionFunction != curFunction){
            duration = 0f;
            transitioning = true;
            curFunction = kernelFunction;
        }
        
        float step = 2f / resolution;
        computeShader.SetInt(resolutionId, resolution);
        computeShader.SetFloat(stepId, step);
        computeShader.SetFloat(timeId, Time.time);
        if(transitioning){
            duration += Time.deltaTime;
            computeShader.SetFloat(progressId, Mathf.SmoothStep(0f, 1f, duration / transitionDuration));
        }
        
        var kernel = (int)curFunction * 5 + (int)(transitioning ? transitionFunction : curFunction);
        int groups = Mathf.CeilToInt(resolution / 8f);
        computeShader.SetBuffer(kernel, positionsId, positionsBuffer);
        computeShader.Dispatch(kernel, groups, groups, 1);

        material.SetBuffer(positionsId, positionsBuffer);
        material.SetVector(scaleId, new Vector4(step, 1f / step));

        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsBuffer);

        if(transitioning && duration >= transitionDuration){
            duration = 0f;
            transitioning = false;
            curFunction = transitionFunction;
        }
    }

    private void Update()
    {
        UpdateFunctionOnGPU();
    }
}