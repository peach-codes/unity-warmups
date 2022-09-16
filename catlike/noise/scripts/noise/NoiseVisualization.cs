using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Noise;
public class NoiseVisualization : Visualization {


    static int noiseId = Shader.PropertyToID("_Noise");

    // who woulda thought folders would fix this...
    static ScheduleDelegate[,] noiseJobs = {
        {
            Job<Lattice1D<LatticeNormal, Perlin>>.ScheduleParrallel,
            Job<Lattice1D<LatticeTiling, Perlin>>.ScheduleParrallel,
            Job<Lattice2D<LatticeNormal, Perlin>>.ScheduleParrallel,
            Job<Lattice2D<LatticeTiling, Perlin>>.ScheduleParrallel,
            Job<Lattice3D<LatticeNormal, Perlin>>.ScheduleParrallel,
            Job<Lattice3D<LatticeTiling, Perlin>>.ScheduleParrallel
        },
        {
			Job<Lattice1D<LatticeNormal, Turbulence<Perlin>>>.ScheduleParrallel,
            Job<Lattice1D<LatticeTiling, Turbulence<Perlin>>>.ScheduleParrallel,
			Job<Lattice2D<LatticeNormal, Turbulence<Perlin>>>.ScheduleParrallel,
            Job<Lattice2D<LatticeTiling, Turbulence<Perlin>>>.ScheduleParrallel,
			Job<Lattice3D<LatticeNormal, Turbulence<Perlin>>>.ScheduleParrallel,
            Job<Lattice3D<LatticeTiling, Turbulence<Perlin>>>.ScheduleParrallel
		},
        {
            Job<Lattice1D<LatticeNormal, Value>>.ScheduleParrallel,
            Job<Lattice1D<LatticeTiling, Value>>.ScheduleParrallel,
            Job<Lattice2D<LatticeNormal, Value>>.ScheduleParrallel,
            Job<Lattice2D<LatticeTiling, Value>>.ScheduleParrallel,
            Job<Lattice3D<LatticeNormal, Value>>.ScheduleParrallel,
            Job<Lattice3D<LatticeTiling, Value>>.ScheduleParrallel
        },
        {
			Job<Lattice1D<LatticeNormal, Turbulence<Value>>>.ScheduleParrallel,
            Job<Lattice1D<LatticeTiling, Turbulence<Value>>>.ScheduleParrallel,
			Job<Lattice2D<LatticeNormal, Turbulence<Value>>>.ScheduleParrallel,
            Job<Lattice2D<LatticeTiling, Turbulence<Value>>>.ScheduleParrallel,
			Job<Lattice3D<LatticeNormal,Turbulence<Value>>>.ScheduleParrallel,
            Job<Lattice3D<LatticeTiling, Turbulence<Value>>>.ScheduleParrallel,
		},
        {
			Job<Voronoi1D<LatticeNormal, Worley, F1>>.ScheduleParrallel,
			Job<Voronoi1D<LatticeTiling, Worley, F1>>.ScheduleParrallel,
			Job<Voronoi2D<LatticeNormal, Worley, F1>>.ScheduleParrallel,
			Job<Voronoi2D<LatticeTiling, Worley, F1>>.ScheduleParrallel,
			Job<Voronoi3D<LatticeNormal, Worley, F1>>.ScheduleParrallel,
			Job<Voronoi3D<LatticeTiling, Worley, F1>>.ScheduleParrallel
		},
        {
			Job<Voronoi1D<LatticeNormal, Worley, F2>>.ScheduleParrallel,
			Job<Voronoi1D<LatticeTiling, Worley, F2>>.ScheduleParrallel,
			Job<Voronoi2D<LatticeNormal, Worley, F2>>.ScheduleParrallel,
			Job<Voronoi2D<LatticeTiling, Worley, F2>>.ScheduleParrallel,
			Job<Voronoi3D<LatticeNormal, Worley, F2>>.ScheduleParrallel,
			Job<Voronoi3D<LatticeTiling, Worley, F2>>.ScheduleParrallel
        },
        {
			Job<Voronoi1D<LatticeNormal, Worley, F2MinusF1>>.ScheduleParrallel,
			Job<Voronoi1D<LatticeTiling, Worley, F2MinusF1>>.ScheduleParrallel,
			Job<Voronoi2D<LatticeNormal, Worley, F2MinusF1>>.ScheduleParrallel,
			Job<Voronoi2D<LatticeTiling, Worley, F2MinusF1>>.ScheduleParrallel,
			Job<Voronoi3D<LatticeNormal, Worley, F2MinusF1>>.ScheduleParrallel,
			Job<Voronoi3D<LatticeTiling, Worley, F2MinusF1>>.ScheduleParrallel
		},
        {
			Job<Voronoi1D<LatticeNormal, Chebyshev, F1>>.ScheduleParrallel,
			Job<Voronoi1D<LatticeTiling, Chebyshev, F1>>.ScheduleParrallel,
			Job<Voronoi2D<LatticeNormal, Chebyshev, F1>>.ScheduleParrallel,
			Job<Voronoi2D<LatticeTiling, Chebyshev, F1>>.ScheduleParrallel,
			Job<Voronoi3D<LatticeNormal, Chebyshev, F1>>.ScheduleParrallel,
			Job<Voronoi3D<LatticeTiling, Chebyshev, F1>>.ScheduleParrallel
		},
        {
			Job<Voronoi1D<LatticeNormal, Chebyshev, F2>>.ScheduleParrallel,
			Job<Voronoi1D<LatticeTiling, Chebyshev, F2>>.ScheduleParrallel,
			Job<Voronoi2D<LatticeNormal, Chebyshev, F2>>.ScheduleParrallel,
			Job<Voronoi2D<LatticeTiling, Chebyshev, F2>>.ScheduleParrallel,
			Job<Voronoi3D<LatticeNormal, Chebyshev, F2>>.ScheduleParrallel,
			Job<Voronoi3D<LatticeTiling, Chebyshev, F2>>.ScheduleParrallel
		},
        {
			Job<Voronoi1D<LatticeNormal, Chebyshev, F2MinusF1>>.ScheduleParrallel,
			Job<Voronoi1D<LatticeTiling, Chebyshev, F2MinusF1>>.ScheduleParrallel,
			Job<Voronoi2D<LatticeNormal, Chebyshev, F2MinusF1>>.ScheduleParrallel,
			Job<Voronoi2D<LatticeTiling, Chebyshev, F2MinusF1>>.ScheduleParrallel,
			Job<Voronoi3D<LatticeNormal, Chebyshev, F2MinusF1>>.ScheduleParrallel,
			Job<Voronoi3D<LatticeTiling, Chebyshev, F2MinusF1>>.ScheduleParrallel
		},
        
    };

    [SerializeField, Range(1,3)]
    int dimensions = 3;

    [SerializeField]
    Settings noiseSettings = Settings.Default;

    [SerializeField]
    SpaceTRS domain = new SpaceTRS {
        scale = 8f
    };

    public enum NoiseType {Perlin, PerlinTurbulence, Value, ValueTurbulence,
        VoronoiWorleyF1, VoroniWorleyF2, VoroniWorleyF2MinusF1,
        VoronoiChebyshevF1, VoronoiChebyshevF2, VoronoiChebyshevF2MinusF1}
        
    [SerializeField]
    NoiseType type;

    [SerializeField]
    bool tiling;
    
    
    NativeArray<float4> noise;

    ComputeBuffer noiseBuffer;




    protected override void EnableVisualization(int dataLength, MaterialPropertyBlock propertyBlock) {
        noise = new NativeArray<float4>(dataLength, Allocator.Persistent);
        noiseBuffer = new ComputeBuffer(dataLength * 4, 4);

        propertyBlock.SetBuffer(noiseId, noiseBuffer);
    }

    protected override void DisableVisualization() {
        noise.Dispose();
        noiseBuffer.Release();
        noiseBuffer = null;
    }

    protected override void UpdateVisualization(
        NativeArray<float3x4> positions, int resolution, JobHandle handle
    ) {
        noiseJobs[(int)type, 2 * dimensions - (tiling? 1 : 2)](
            positions, noise, noiseSettings, domain, resolution, handle
        ).Complete();
        noiseBuffer.SetData(noise.Reinterpret<float>(4 * 4));
    }

}

