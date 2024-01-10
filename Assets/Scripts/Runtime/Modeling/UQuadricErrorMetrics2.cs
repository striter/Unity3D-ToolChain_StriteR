using System.Collections.Generic;
using System.Linq;
using Runtime.Geometry;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace QuadricErrorsMetric
{
	//&https://github.com/sp4cerat/Fast-Quadric-Mesh-Simplification/blob/master/src.cmd/Simplify.h#L768
    public class UQuadricErrorMetrics2
    {
	    public class Triangle { 
			public int[] v = new int[3];
			public float[] err = new float[4];
			public bool deleted,dirty,attr;
			public float3 n;
			public float3[] uvs=new float3[3];
		};

	    public class Vertex
		{
			public float3 p;
			public int tstart,tcount;
			public float4x4_symmetric q;
			public bool border;
		};

		struct Ref
		{
			public int tid,tvertex;
		};

		public List<Triangle> triangles { get; private set; } = new();
		public List<Vertex> vertices { get; private set; } = new();
		List<Ref> refs = new ();

		public void Init(Mesh _sharedMesh)//, bool process_uv=false)
        {
			vertices.Clear();
			triangles.Clear();

			vertices.AddRange(_sharedMesh.vertices.Select(p=> new Vertex() {
				p = p,
			}));

			
			foreach (var p in _sharedMesh.GetPolygons(out var indexes))
			{
				triangles.Add(new Triangle() {
					v = new int[]{p.V0,p.V1,p.V2},
					n = GPlane.FromPositions(vertices[p.V0].p,vertices[p.V1].p,vertices[p.V2].p).normal,
				});
			}
			update_mesh(0);
		} // load_obj()

			
		public void PopulateMesh(Mesh _mesh)
		{
			_mesh.Clear();
			_mesh.SetVertices(vertices.Select(p => (Vector3)p.p).ToList());
			_mesh.SetTriangles(triangles.Select(p=>(IEnumerable<int>)p.v).Resolve().ToArray(), 0);
		}

		public void DrawGizmos()
		{
			foreach (var triangle in triangles)
				UGizmos.DrawLinesConcat(triangle.v.Select(p=>vertices[p].p));
		}
		
		public void simplify_mesh(int target_count, float agressiveness=7, bool verbose=false)
		{
			// init
			for(var i =0;i < triangles.Count;i++)
	            triangles[i].deleted=false;

			// main iteration loop
			int deleted_triangles=0;
			List<bool> deleted0 = new (),deleted1 = new ();
			int triangle_count=triangles.Count;
			//int iteration = 0;
			//loop(iteration,0,100)
			for (int iteration = 0; iteration < 100; iteration ++)
			{
				if(triangle_count-deleted_triangles<=target_count)
					break;

				// update mesh once in a while
				if(iteration%5==0)
					update_mesh(iteration);

				// clear dirty flag
				for(var i=0;i <triangles.Count;i++) triangles[i].dirty=false;

				//
				// All triangles with edges below the threshold will be removed
				//
				// The following numbers works well for most models.
				// If it does not, try to adjust the 3 parameters
				//
				double threshold = 0.000000001*pow(iteration+3,agressiveness);

				// target number of triangles reached ? Then break
				if (verbose && iteration%5==0) 
					Debug.Log($"iteration {iteration} - triangles {triangle_count-deleted_triangles} threshold {threshold}");

				// remove vertices & mark deleted triangles
				for(var i=0;i<triangles.Count;i++)
				{
					var t=triangles[i];
					if(t.err[3]>threshold) continue;
					if(t.deleted) continue;
					if(t.dirty) continue;

					for(var j =0;j<3;j++)if(t.err[j]<threshold)
					{

						var i0=t.v[ j     ]; var v0 = vertices[i0];
						var i1=t.v[(j+1)%3]; var v1 = vertices[i1];
						// Border check
						if(v0.border != v1.border)  continue;

						// Compute vertex to collapse to
						calculate_error(i0,i1,out var p);
						deleted0.Resize(v0.tcount); // normals temporarily
						deleted1.Resize(v1.tcount); // normals temporarily
						// don't remove if flipped
						if( flipped(p,i0,i1,v0,v1,deleted0) ) continue;

						if( flipped(p,i1,i0,v1,v0,deleted1) ) continue;

						// if ( (t.attr & TEXCOORD) == TEXCOORD  )
						// {
						// 	update_uvs(i0,v0,p,deleted0);
						// 	update_uvs(i0,v1,p,deleted1);
						// }

						// not flipped, so remove edge
						v0.p=p;
						v0.q=v1.q+v0.q;
						int tstart=refs.Count;

						update_triangles(i0,v0,deleted0,ref deleted_triangles);
						update_triangles(i0,v1,deleted1,ref deleted_triangles);

						int tcount=refs.Count-tstart;

						if(tcount<=v0.tcount)
						{
							// save ram
							for (int k = 0; k < tcount; k++)
								refs[v0.tstart + k] = refs[tstart + k];
						}
						else
							// append
							v0.tstart=tstart;

						v0.tcount=tcount;
						break;
					}
					// done?
					if(triangle_count-deleted_triangles<=target_count)
						break;
				}
			}
			// clean up mesh
			compact_mesh();
		} //simplify_mesh()

		// void simplify_mesh_lossless(bool verbose=false)
		// {
		// 	// init
		// 	for(int i=0;i<triangles.Count;i++)
		// 		triangles[i].deleted=false;
		//
		// 	// main iteration loop
		// 	int deleted_triangles=0;
		// 	List<bool> deleted0= new (),deleted1= new ();
		// 	int triangle_count=triangles.Count;
		// 	//int iteration = 0;
		// 	//loop(iteration,0,100)
		// 	for (int iteration = 0; iteration < 9999; iteration ++)
		// 	{
		// 		// update mesh constantly
		// 		update_mesh(iteration);
		// 		// clear dirty flag
		// 		for(int i=0;i<triangles.Count;i++)
		// 			triangles[i].dirty=false;
		// 		//
		// 		// All triangles with edges below the threshold will be removed
		// 		//
		// 		// The following numbers works well for most models.
		// 		// If it does not, try to adjust the 3 parameters
		// 		//
		// 		double threshold = float.Epsilon; //1.0E-3 EPS;
		// 		if (verbose)
		// 			Debug.Log($"lossless iteration {iteration}");
		//
		// 		// remove vertices & mark deleted triangles
		// 		for(int i=0;i<triangles.Count;i++)
		// 		{
		// 			var t=triangles[i];
		// 			if(t.err[3]>threshold) continue;
		// 			if(t.deleted) continue;
		// 			if(t.dirty) continue;
		//
		// 			for(int j=0;j<3;j++)
		// 				if(t.err[j]<threshold)
		// 				{
		// 					int i0=t.v[ j     ]; var v0 = vertices[i0];
		// 					int i1=t.v[(j+1)%3]; var v1 = vertices[i1];
		//
		// 					// Border check
		// 					if(v0.border != v1.border)  continue;
		//
		// 					// Compute vertex to collapse to
		// 					calculate_error(i0,i1,out var p);
		//
		// 					deleted0.Resize(v0.tcount); // normals temporarily
		// 					deleted1.Resize(v1.tcount); // normals temporarily
		//
		// 					// don't remove if flipped
		// 					if( flipped(p,i0,i1,v0,v1,deleted0) ) continue;
		// 					if( flipped(p,i1,i0,v1,v0,deleted1) ) continue;
		// 					//
		// 					// if ( (t.attr & TEXCOORD) == TEXCOORD )
		// 					// {
		// 					// 	update_uvs(i0,v0,p,deleted0);
		// 					// 	update_uvs(i0,v1,p,deleted1);
		// 					// }
		//
		// 					// not flipped, so remove edge
		// 					v0.p=p;
		// 					v0.q=v1.q+v0.q;
		// 					int tstart=refs.Count;
		//
		// 					update_triangles(i0,v0,deleted0,ref deleted_triangles);
		// 					update_triangles(i0,v1,deleted1,ref deleted_triangles);
		//
		// 					int tcount=refs.Count-tstart;
		//
		// 					if(tcount<=v0.tcount)
		// 					{
		// 						// save ram
		// 						for (int k = 0; k < tcount; k++)
		// 							refs[v0.tstart + k] = refs[tstart + k];
		// 					}
		// 					else
		// 						// append
		// 						v0.tstart=tstart;
		//
		// 					v0.tcount=tcount;
		// 					break;
		// 				}
		// 		}
		// 		if(deleted_triangles<=0)break;
		// 		deleted_triangles=0;
		// 	} //for each iteration
		// 	// clean up mesh
		// 	compact_mesh();
		// } //simplify_mesh_lossless()
		//

		// Check if a triangle flips when this edge is removed

		bool flipped(float3 p,int i0,int i1,Vertex v0,Vertex v1,List<bool> deleted)
		{

			for (int k = 0;k<v0.tcount;k++)
			{
				var t=triangles[refs[v0.tstart+k].tid];
				if(t.deleted)continue;

				int s=refs[v0.tstart+k].tvertex;
				int id1=t.v[(s+1)%3];
				int id2=t.v[(s+2)%3];

				if(id1==i1 || id2==i1) // delete ?
				{

					deleted[k]=true;
					continue;
				}
				var d1 = vertices[id1].p-p;d1 = d1.normalize();
				var d2 = vertices[id2].p-p;d2 = d2.normalize();
				if(abs(d1.dot(d2))>0.999) return true;
				var n = cross(d1,d2);
				n = n.normalize();
				deleted[k]=false;
				if(n.dot(t.n)<0.2) return true;
			}
			return false;
		}

	    // update_uvs

		// void update_uvs(int i0,Vertex v,float3 p,List<bool> deleted)
		// {
		// 	for(var k=0;k<v.tcount;k++)
		// 	{
		// 		var r=refs[v.tstart+k];
		// 		var t=triangles[r.tid];
		// 		if(t.deleted)continue;
		// 		if(deleted[k])continue;
		// 		var p1=vertices[t.v[0]].p;
		// 		var p2=vertices[t.v[1]].p;
		// 		var p3=vertices[t.v[2]].p;
		// 		t.uvs[r.tvertex] = interpolate(p,p1,p2,p3,t.uvs);
		// 	}
		// }

		// Update triangle connections and edge error after a edge is collapsed

		void update_triangles(int i0,Vertex v,List<bool> deleted,ref int deleted_triangles)
		{
			for(var k = 0;k <v.tcount;k++)
			{
				var r=refs[v.tstart+k];
				var t=triangles[r.tid];
				if(t.deleted)continue;
				if(deleted[k])
				{
					t.deleted=true;
					deleted_triangles++;
					continue;
				}
				t.v[r.tvertex]=i0;
				t.dirty=true;
				t.err[0]=calculate_error(t.v[0],t.v[1],out _);
				t.err[1]=calculate_error(t.v[1],t.v[2],out _);
				t.err[2]=calculate_error(t.v[2],t.v[0],out _);
				t.err[3]=min(t.err[0],min(t.err[1],t.err[2]));
				refs.Add(r);
			}
		}

		// compact triangles, compute edge error and build reference list

		void update_mesh(int iteration)
		{
			if(iteration>0) // compact triangles
			{
				int dst=0;
				for(int i=0;i<triangles.Count;i++)
					if(!triangles[i].deleted)
					{
						triangles[dst++]=triangles[i];
					}
				triangles.Resize(dst);
			}
			//

			// Init Reference ID list
			for(var i=0;i<vertices.Count;i++)	
			{
				vertices[i].tstart=0;
				vertices[i].tcount=0;
			}
			for(var i=0;i<triangles.Count;i++)
			{
				var t=triangles[i];
				for(var j =0;j<3;j++) vertices[t.v[j]].tcount++;
			}
			int tstart=0;
			for(var i=0;i<vertices.Count;i++)
			{
				var v=vertices[i];
				v.tstart=tstart;
				tstart+=v.tcount;
				v.tcount=0;
			}

			// Write References
			refs.Resize(triangles.Count*3);
			for(var i=0;i<triangles.Count;i++)
			{
				var t=triangles[i];
				for(var j=0;j<3;j++)
				{
					var v=vertices[t.v[j]];
					refs[v.tstart + v.tcount] = new()
					{
						tid = i,
						tvertex = j
					};
					v.tcount++;
				}
			}

			// Init Quadrics by Plane & Edge Errors
			//
			// required at the beginning ( iteration == 0 )
			// recomputing during the simplification is not required,
			// but mostly improves the result for closed meshes
			//
			if( iteration == 0 )
			{
				// Identify boundary : vertices[].border=0,1

				List<int> vcount= new (),vids = new ();

				for(var i=0;i<vertices.Count;i++)
					vertices[i].border=false;

				for(var i=0;i<vertices.Count;i++)
				{
					var v=vertices[i];
					vcount.Clear();
					vids.Clear();
					for(var j=0;j<v.tcount;j++)
					{
						int tIndex=refs[v.tstart+j].tid;
						var t=triangles[tIndex];
						for(var k=0;k<3;k++)
						{
							int ofs=0,id=t.v[k];
							while(ofs<vcount.Count)
							{
								if(vids[ofs]==id)break;
								ofs++;
							}
							if(ofs==vcount.Count)
							{
								vcount.Add(1);
								vids.Add(id);
							}
							else
								vcount[ofs]++;
						}
					}
					for(int j=0;j<vcount.Count;j++)
						 if(vcount[j]==1)
							vertices[vids[j]].border=true;
				}
				//initialize errors
				for(int i=0;i<vertices.Count;i++)
					vertices[i].q= float4x4_symmetric.zero;

				for(int i=0;i<vertices.Count;i++)
				{
					var t=triangles[i];
					var plane = GPlane.FromPositions(vertices[t.v[0]].p,
					vertices[t.v[1]].p,
					vertices[t.v[2]].p);
					for (var j = 0; j < 3; j++)
						vertices[t.v[j]].q += CalculateErrorMatrix(plane);
				}
				for(int i=0;i<triangles.Count;i++)
				{
					// Calc Edge Error
					var t=triangles[i];
					for(var j =0;j<3;j++)
					    t.err[j]=calculate_error(t.v[j],t.v[(j+1)%3],out var p);
					t.err[3]=min(t.err[0],min(t.err[1],t.err[2]));
				}
			}
		}

		// Finally compact mesh before exiting

		void compact_mesh()
		{
			int dst=0;
			for(int i=0;i<vertices.Count;i++)
				vertices[i].tcount=0;
			for(int i=0;i<triangles.Count;i++)
				if(!triangles[i].deleted)
				{
					var t=triangles[i];
					triangles[dst++]=t;
					for(var j =0;j<3;j++)
					    vertices[t.v[j]].tcount=1;
				}
			triangles.Resize(dst);
			dst=0;
			for(int i=0;i<vertices.Count;i++)
				if(vertices[i].tcount > 0)
				{
					vertices[i].tstart=dst;
					vertices[dst].p=vertices[i].p;
					dst++;
				}
			for(int i=0;i<triangles.Count;i++)
			{
				var t=triangles[i];
				for(var j =0;j<3;j++)
					t.v[j]=vertices[t.v[j]].tstart;
			}
			vertices.Resize(dst);
		}

		// Error between vertex and Quadric

		float vertex_error(float4x4_symmetric q, float x, float y, float z)
		{
 			return   q.Index(0)*x*x + 2*q.Index(1)*x*y + 2*q.Index(2)*x*z + 2*q.Index(3)*x + q.Index(4)*y*y
 			     + 2*q.Index(5)*y*z + 2*q.Index(6)*y + q.Index(7)*z*z + 2*q.Index(8)*z + q.Index(9);
		}

		// Error for one edge

		float calculate_error(int id_v1, int id_v2, out float3 p_result)
		{
			p_result = default;
			// compute interpolated vertex
			var q = vertices[id_v1].q + vertices[id_v2].q;
			var   border = vertices[id_v1].border && vertices[id_v2].border;
			var error=0f;
			var det = q.determinant(0, 1, 2, 1, 4, 5, 2, 5, 7);
			if ( det != 0 && !border )
			{

				// q_delta is invertible
				p_result.x = -1f/det*(q.determinant(1, 2, 3, 4, 5, 6, 5, 7 , 8));	// vx = A41/det(q_delta)
				p_result.y =  1f/det*(q.determinant(0, 2, 3, 1, 5, 6, 2, 7 , 8));	// vy = A42/det(q_delta)
				p_result.z = -1f/det*(q.determinant(0, 1, 3, 1, 4, 6, 2, 5,  8));	// vz = A43/det(q_delta)

				error = vertex_error(q, p_result.x, p_result.y, p_result.z);
			}
			else
			{
				// det = 0 -> try to find best result
				var p1=vertices[id_v1].p;
				var p2=vertices[id_v2].p;
				var p3=(p1+p2)/2;
				var error1 = vertex_error(q, p1.x,p1.y,p1.z);
				var error2 = vertex_error(q, p2.x,p2.y,p2.z);
				var error3 = vertex_error(q, p3.x,p3.y,p3.z);
				error = min(error1, min(error2, error3));
				if (error1 == error) p_result=p1;
				if (error2 == error) p_result=p2;
				if (error3 == error) p_result=p3;
			}
			return error;
		}


		public float4x4_symmetric CalculateErrorMatrix(GPlane _plane)
		{
			var a = _plane.position.x;
			var b = _plane.position.y;
			var c = _plane.position.z;
			var d = -_plane.distance;

			
			return new float4x4_symmetric(
				a*a,a*b, a*c, a*d,
				b*b, b*c, b*d,
				c*c, c*d,
				d*d
			);
		}

    }
}