using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

namespace ktest{
    [ExecuteInEditMode]
    public class CSMatExample : MonoBehaviour
    {
        void Awake(){
            
        }

        private bool isStart = false;
        void Start(){
            if(Application.isPlaying){
                isStart = true;
                TestFunc();
            }
        }

        void OnEnable(){
            if(isStart){
                TestFunc();
            }
        }

        void OnDisable(){
            ResetFunc();
        }

        void Update(){
            UpdateFunc();
        }
        
        void OnDestroy(){
            isStart = false;
        }

        /*************** context funcs ***************/
        public GameObject originCub = null;
        public ComputeShader computeShader = default;

        public int fabLevel = 4;
        public float rspeed = 0.5f;
        
        [ContextMenu("TestFunc")]
        public void TestFunc(){
            if(originCub == null) return;
            
            ResetFunc();
            CreateFabCubesGPU(fabLevel, -1, null, transform);
        }
        
        [ContextMenu("ResetFunc")]
        public void ResetFunc(){
            rmesh = null;
            preFabLevel = fabLevel;
            preRoSpeed = rspeed;

            if(gpuInfos != null)
                gpuInfos.Clear();

            if(lvInfos != null)
                lvInfos.Clear();

            ClearBuffer(ref argsBuffer);
            ClearBuffer(ref lmats);
            ClearBuffer(ref matBuffer);
            ClearBuffer(ref idmats);
            ClearBuffer(ref argsBuffer);
        }

        private void ClearBuffer(ref ComputeBuffer buffer){
            if(buffer != null){
                buffer.Release();
                buffer = null;
            }
        }

        private void UpdateFunc(){
            if(originCub == null) return;
            UpdateCubes();
        }

        private Mesh rmesh = null;
        private Material rmat = null;
        private Matrix4x4 rmatri = Matrix4x4.identity;

        private ComputeBuffer matBuffer = null, argsBuffer = null;
        private uint[] args = new uint[]{0, 0, 0, 0, 0, 0};

        static readonly int
            lmatId = Shader.PropertyToID("_lmats"),
            wmatId = Shader.PropertyToID("_wmats"),
            matIdxId = Shader.PropertyToID("_matIdxs"),
            idSets = Shader.PropertyToID("_ids"),
            idGroup = Shader.PropertyToID("_group"),
            roMatId = Shader.PropertyToID("_roMat");

        
        ComputeBuffer lmats = null, idmats = null;
        Dictionary<int, Vector2Int> gpuIdset = null;
        List<GPUCubeInfo> gpuInfos = null;

        private int preFabLevel = 0;
        private float preRoSpeed = 0f;
        private void UpdateCubes(){
            if(lvInfos.Count == 0) return;
            if(preFabLevel != fabLevel){
                ResetFunc();
            }

            if(rmesh == null){
                rmesh = originCub.GetComponent<MeshFilter>().sharedMesh;
                rmat = originCub.GetComponent<MeshRenderer>().sharedMaterial;

                gpuIdset = new Dictionary<int, Vector2Int>();
                gpuInfos = new List<GPUCubeInfo>();
                foreach(var p in lvInfos){
                    var s = gpuInfos.Count;
                    gpuInfos.AddRange(p.Value);
                    gpuIdset.Add(p.Key, new Vector2Int(s, gpuInfos.Count - 1));
                }
                
                args[0] = rmesh.GetIndexCount(0);
                args[1] = (uint)gpuInfos.Count;
                argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
                argsBuffer.SetData(args);
                
                rmatri = Matrix4x4.Rotate(Quaternion.Euler(0, rspeed, 0f));
                matBuffer = new ComputeBuffer(gpuInfos.Count, 64);
                idmats = new ComputeBuffer(gpuInfos.Count, 4);
                lmats = new ComputeBuffer(gpuInfos.Count, 64);
                
                var ids = new List<int>(gpuInfos.Count);
                var matsl = new List<Matrix4x4>(gpuInfos.Count);
                var matsw = new List<Matrix4x4>(gpuInfos.Count);
                for(int i = 0; i < gpuInfos.Count; ++i){
                    var info = gpuInfos[i];
                    info.id = i;
                    
                    ids.Add(info.pinfo == null ? -1 : info.pinfo.id);
                    matsl.Add(info.mat);
                    matsw.Add(info.mat);
                }
                
                idmats.SetData<int>(ids);
                lmats.SetData<Matrix4x4>(matsl);
                matBuffer.SetData<Matrix4x4>(matsw);

                computeShader.SetMatrix(roMatId, rmatri);
                computeShader.SetBuffer(0, matIdxId, idmats);
                
                computeShader.SetBuffer(0, wmatId, matBuffer);
                computeShader.SetBuffer(0, lmatId, lmats);
            } else if(preRoSpeed != rspeed){
                preRoSpeed = rspeed;
                rmatri = Matrix4x4.Rotate(Quaternion.Euler(0, rspeed, 0f));
                computeShader.SetMatrix(roMatId, rmatri);
            }
            
            for(int i = 0; i < fabLevel; ++i){
                var idt = new int[]{ gpuIdset[i].x, gpuIdset[i].y };
                int g = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt((idt[1] + 1) / 64)));
                
                computeShader.SetInts(idGroup, g);
                computeShader.SetInts(idSets, idt);
                computeShader.Dispatch(0, g, g, 1);
            }
            
            rmat.SetBuffer("_mats", matBuffer);
            Graphics.DrawMeshInstancedIndirect(rmesh, 0, rmat, new Bounds(Vector3.zero, Vector3.one * 2), argsBuffer);
        }

        private Dictionary<int, List<GPUCubeInfo>> lvInfos = new Dictionary<int, List<GPUCubeInfo>>();
        private void AddToLvInfo(GPUCubeInfo info, int fab){
            KUtils.GetListInDict(lvInfos, fab).Add(info);
        }

        private void CreateFabCubesGPU(int ifab, int idx, GPUCubeInfo pinfo = null, Transform con = null){
            if(ifab <= 0) return;
            
            GPUCubeInfo ginfo = null;
            Matrix4x4 smat = Matrix4x4.identity;
            if(idx == -1){
                smat = con.localToWorldMatrix * Matrix4x4.Scale(Vector3.one * ifab);
                AddToLvInfo(ginfo = new GPUCubeInfo(null, smat), fabLevel - ifab);
                for(int i = 0; i < 6; ++i)
                    CreateFabCubesGPU(ifab - 1, i, ginfo);
                return;
            }
            
            switch(idx){
                case 0:
                    smat = Matrix4x4.Translate(new Vector3(0, 0.75f));
                    break;
                case 1:
                    smat = Matrix4x4.Translate(new Vector3(0, 0, -0.75f));
                    smat *= Matrix4x4.Rotate(Quaternion.Euler(-90, 0, 0));
                    break;
                case 2:
                    smat = Matrix4x4.Translate(new Vector3(-0.75f, 0));
                    smat *= Matrix4x4.Rotate(Quaternion.Euler(0, 0, 90f));
                    break;
                case 3:
                    smat = Matrix4x4.Translate(new Vector3(0.75f, 0));
                    smat *= Matrix4x4.Rotate(Quaternion.Euler(0, 0, -90f));
                    break;
                case 4:
                    smat = Matrix4x4.Translate(new Vector3(0, 0, 0.75f));
                    smat *= Matrix4x4.Rotate(Quaternion.Euler(90f, 0, 0));
                    break;
                case 5:
                    smat = Matrix4x4.Translate(new Vector3(0, -0.75f, 0));
                    smat *= Matrix4x4.Rotate(Quaternion.Euler(180f, 0, 0));
                    break;
            }
            
            smat *= Matrix4x4.Scale(Vector3.one * 0.5f);
            AddToLvInfo(ginfo = new GPUCubeInfo(pinfo, smat), fabLevel - ifab);
            for(int i = 0; i < 5; ++i){
                CreateFabCubesGPU(ifab - 1, i, ginfo);
            }
        }

        public class GPUCubeInfo {
            public GPUCubeInfo pinfo = null;
            public Matrix4x4 mat = Matrix4x4.identity;
            public int id = 0;
            public GPUCubeInfo(GPUCubeInfo pinfo, Matrix4x4 mat){
                this.mat = mat;
                this.pinfo = pinfo;
            }
        }

        [ContextMenu("FindKernel")]
        public void FindKernel(){
            Debug.LogWarning(computeShader.FindKernel("MatKernel"));
        }
    }
}