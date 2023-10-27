using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace QuadricErrorsMetric
{
	//&https://github.com/sp4cerat/Fast-Quadric-Mesh-Simplification/blob/master/src.cmd/Simplify.h#L768
    public class UQuadricErrorMetrices2
    {
		class Triangle { 
			public int[] v = new int[3];
			public float[] err = new float[4];
			public bool deleted,dirty,attr;
			public float3 n;
			public float3[] uvs=new float3[3];
		};

		class Vertex
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
		
		List<Triangle> triangles = new();
		List<Vertex> vertices = new();
		List<Ref> refs = new ();

		void simplify_mesh(int target_count, double agressiveness=7, bool verbose=false)
		{
			// init
			for(var i =0;i < triangles.Count;i++)
	            triangles[i].deleted=false;

			// main iteration loop
			int deleted_triangles=0;
			List<int> deleted0 = new (),deleted1 = new ();
			int triangle_count=triangles.Count();
			//int iteration = 0;
			//loop(iteration,0,100)
			for (int iteration = 0; iteration < 100; iteration ++)
			{
				if(triangle_count-deleted_triangles<=target_count)break;

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

						if ( (t.attr & TEXCOORD) == TEXCOORD  )
						{
							update_uvs(i0,v0,p,deleted0);
							update_uvs(i0,v1,p,deleted1);
						}

						// not flipped, so remove edge
						v0.p=p;
						v0.q=v1.q+v0.q;
						int tstart=refs.size();

						update_triangles(i0,v0,deleted0,deleted_triangles);
						update_triangles(i0,v1,deleted1,deleted_triangles);

						int tcount=refs.size()-tstart;

						if(tcount<=v0.tcount)
						{
							// save ram
							if(tcount)memcpy(&refs[v0.tstart],&refs[tstart],tcount*sizeof(Ref));
						}
						else
							// append
							v0.tstart=tstart;

						v0.tcount=tcount;
						break;
					}
					// done?
					if(triangle_count-deleted_triangles<=target_count)break;
				}
			}
			// clean up mesh
			compact_mesh();
		} //simplify_mesh()

		void simplify_mesh_lossless(bool verbose=false)
		{
			// init
			loopi(0,triangles.size()) triangles[i].deleted=0;

			// main iteration loop
			int deleted_triangles=0;
			std::vector<int> deleted0,deleted1;
			int triangle_count=triangles.size();
			//int iteration = 0;
			//loop(iteration,0,100)
			for (int iteration = 0; iteration < 9999; iteration ++)
			{
				// update mesh constantly
				update_mesh(iteration);
				// clear dirty flag
				loopi(0,triangles.size()) triangles[i].dirty=0;
				//
				// All triangles with edges below the threshold will be removed
				//
				// The following numbers works well for most models.
				// If it does not, try to adjust the 3 parameters
				//
				double threshold = DBL_EPSILON; //1.0E-3 EPS;
				if (verbose) {
					printf("lossless iteration %d\n", iteration);
				}

				// remove vertices & mark deleted triangles
				loopi(0,triangles.size())
				{
					Triangle &t=triangles[i];
					if(t.err[3]>threshold) continue;
					if(t.deleted) continue;
					if(t.dirty) continue;

					loopj(0,3)if(t.err[j]<threshold)
					{
						int i0=t.v[ j     ]; Vertex &v0 = vertices[i0];
						int i1=t.v[(j+1)%3]; Vertex &v1 = vertices[i1];

						// Border check
						if(v0.border != v1.border)  continue;

						// Compute vertex to collapse to
						vec3f p;
						calculate_error(i0,i1,p);

						deleted0.resize(v0.tcount); // normals temporarily
						deleted1.resize(v1.tcount); // normals temporarily

						// don't remove if flipped
						if( flipped(p,i0,i1,v0,v1,deleted0) ) continue;
						if( flipped(p,i1,i0,v1,v0,deleted1) ) continue;

						if ( (t.attr & TEXCOORD) == TEXCOORD )
						{
							update_uvs(i0,v0,p,deleted0);
							update_uvs(i0,v1,p,deleted1);
						}

						// not flipped, so remove edge
						v0.p=p;
						v0.q=v1.q+v0.q;
						int tstart=refs.size();

						update_triangles(i0,v0,deleted0,deleted_triangles);
						update_triangles(i0,v1,deleted1,deleted_triangles);

						int tcount=refs.size()-tstart;

						if(tcount<=v0.tcount)
						{
							// save ram
							if(tcount)memcpy(&refs[v0.tstart],&refs[tstart],tcount*sizeof(Ref));
						}
						else
							// append
							v0.tstart=tstart;

						v0.tcount=tcount;
						break;
					}
				}
				if(deleted_triangles<=0)break;
				deleted_triangles=0;
			} //for each iteration
			// clean up mesh
			compact_mesh();
		} //simplify_mesh_lossless()


		// Check if a triangle flips when this edge is removed

		bool flipped(float3 p,int i0,int i1,Vertex v0,Vertex v1,List<int> deleted)
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

					deleted[k]=1;
					continue;
				}
				var d1 = vertices[id1].p-p;d1 = d1.normalize();
				var d2 = vertices[id2].p-p;d2 = d2.normalize();
				if(abs(d1.dot(d2))>0.999) return true;
				var n = cross(d1,d2);
				n = n.normalize();
				deleted[k]=0;
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
					loopj(0,vcount.size()) if(vcount[j]==1)
						vertices[vids[j]].border=1;
				}
				//initialize errors
				loopi(0,vertices.size())
					vertices[i].q=SymetricMatrix(0.0);

				loopi(0,triangles.size())
				{
					Triangle &t=triangles[i];
					vec3f n,p[3];
					loopj(0,3) p[j]=vertices[t.v[j]].p;
					n.cross(p[1]-p[0],p[2]-p[0]);
					n.normalize();
					t.n=n;
					loopj(0,3) vertices[t.v[j]].q =
						vertices[t.v[j]].q+SymetricMatrix(n.x,n.y,n.z,-n.dot(p[0]));
				}
				loopi(0,triangles.size())
				{
					// Calc Edge Error
					Triangle &t=triangles[i];vec3f p;
					loopj(0,3) t.err[j]=calculate_error(t.v[j],t.v[(j+1)%3],p);
					t.err[3]=min(t.err[0],min(t.err[1],t.err[2]));
				}
			}
		}

		// Finally compact mesh before exiting

		void compact_mesh()
		{
			int dst=0;
			loopi(0,vertices.size())
			{
				vertices[i].tcount=0;
			}
			loopi(0,triangles.size())
			if(!triangles[i].deleted)
			{
				Triangle &t=triangles[i];
				triangles[dst++]=t;
				loopj(0,3)vertices[t.v[j]].tcount=1;
			}
			triangles.resize(dst);
			dst=0;
			loopi(0,vertices.size())
			if(vertices[i].tcount)
			{
				vertices[i].tstart=dst;
				vertices[dst].p=vertices[i].p;
				dst++;
			}
			loopi(0,triangles.size())
			{
				Triangle &t=triangles[i];
				loopj(0,3)t.v[j]=vertices[t.v[j]].tstart;
			}
			vertices.resize(dst);
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


		//Option : Load OBJ
		void load_obj(const char* filename, bool process_uv=false){
			vertices.clear();
			triangles.clear();
			//printf ( "Loading Objects %s ... \n",filename);
			FILE* fn;
			if(filename==NULL)		return ;
			if((char)filename[0]==0)	return ;
			if ((fn = fopen(filename, "rb")) == NULL)
			{
				printf ( "File %s not found!\n" ,filename );
				return;
			}
			char line[1000];
			memset ( line,0,1000 );
			int vertex_cnt = 0;
			int material = -1;
			std::map<std::string, int> material_map;
			std::vector<vec3f> uvs;
			std::vector<std::vector<int> > uvMap;

			while(fgets( line, 1000, fn ) != NULL)
			{
				Vertex v;
				vec3f uv;

				if (strncmp(line, "mtllib", 6) == 0)
				{
					mtllib = trimwhitespace(&line[7]);
				}
				if (strncmp(line, "usemtl", 6) == 0)
				{
					std::string usemtl = trimwhitespace(&line[7]);
					if (material_map.find(usemtl) == material_map.end())
					{
						material_map[usemtl] = materials.size();
						materials.push_back(usemtl);
					}
					material = material_map[usemtl];
				}

				if ( line[0] == 'v' && line[1] == 't' )
				{
					if ( line[2] == ' ' )
					if(sscanf(line,"vt %lf %lf",
						&uv.x,&uv.y)==2)
					{
						uv.z = 0;
						uvs.push_back(uv);
					} else
					if(sscanf(line,"vt %lf %lf %lf",
						&uv.x,&uv.y,&uv.z)==3)
					{
						uvs.push_back(uv);
					}
				}
				else if ( line[0] == 'v' )
				{
					if ( line[1] == ' ' )
					if(sscanf(line,"v %lf %lf %lf",
						&v.p.x,	&v.p.y,	&v.p.z)==3)
					{
						vertices.push_back(v);
					}
				}
				int integers[9];
				if ( line[0] == 'f' )
				{
					Triangle t;
					bool tri_ok = false;
	                bool has_uv = false;

					if(sscanf(line,"f %d %d %d",
						&integers[0],&integers[1],&integers[2])==3)
					{
						tri_ok = true;
					}else
					if(sscanf(line,"f %d// %d// %d//",
						&integers[0],&integers[1],&integers[2])==3)
					{
						tri_ok = true;
					}else
					if(sscanf(line,"f %d//%d %d//%d %d//%d",
						&integers[0],&integers[3],
						&integers[1],&integers[4],
						&integers[2],&integers[5])==6)
					{
						tri_ok = true;
					}else
					if(sscanf(line,"f %d/%d/%d %d/%d/%d %d/%d/%d",
						&integers[0],&integers[6],&integers[3],
						&integers[1],&integers[7],&integers[4],
						&integers[2],&integers[8],&integers[5])==9)
					{
						tri_ok = true;
						has_uv = true;
					}else // Add Support for v/vt only meshes
					if (sscanf(line, "f %d/%d %d/%d %d/%d",
						&integers[0], &integers[6],
						&integers[1], &integers[7],
						&integers[2], &integers[8]) == 6)
					{
						tri_ok = true;
						has_uv = true;
					}
					else
					{
						printf("unrecognized sequence\n");
						printf("%s\n",line);
						while(1);
					}
					if ( tri_ok )
					{
						t.v[0] = integers[0]-1-vertex_cnt;
						t.v[1] = integers[1]-1-vertex_cnt;
						t.v[2] = integers[2]-1-vertex_cnt;
						t.attr = 0;

						if ( process_uv && has_uv )
						{
							std::vector<int> indices;
							indices.push_back(integers[6]-1-vertex_cnt);
							indices.push_back(integers[7]-1-vertex_cnt);
							indices.push_back(integers[8]-1-vertex_cnt);
							uvMap.push_back(indices);
							t.attr |= TEXCOORD;
						}

						t.material = material;
						//geo.triangles.push_back ( tri );
						triangles.push_back(t);
						//state_before = state;
						//state ='f';
					}
				}
			}

			if ( process_uv && uvs.size() )
			{
				loopi(0,triangles.size())
				{
					loopj(0,3)
					triangles[i].uvs[j] = uvs[uvMap[i][j]];
				}
			}

			fclose(fn);

			//printf("load_obj: vertices = %lu, triangles = %lu, uvs = %lu\n", vertices.size(), triangles.size(), uvs.size() );
		} // load_obj()


    }
}