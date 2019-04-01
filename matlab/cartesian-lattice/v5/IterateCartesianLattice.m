%imXYout = complex-valued tensor (Nx x Ny) for new node positions
function [next_imXYout, next_velocity_imXY_out] = IterateCartesianLattice(position_imXYprev, velocity_imXYprev, forcing_imXY, settings, timeindex)
%This function takes all of the relevant data from the previous iteration and estimates the next system state.

%Initialize some stuff
[Ny, Nx] = size(position_imXYprev);
next_imXYout = zeros(Ny, Nx);
next_velocity_imXY_out = zeros(Nx, Ny);
b = settings.dampingCoefficient;
k = settings.springConstant;
req = settings.equilibriumLength;
timestep = settings.timeStep;

%Over all interior nodes.
for i=1:Ny
   for j = 1:Nx
   
		%This represents the spring 'stencil' in terms of indices. e.g. the value 
		%indices = [0 1 0 -1;1 0 -1 0]; represents connectivity with the upper/lower, left/right nodes.
       indices = [0 1 0 -1;1 0 -1 0];
	   
	   %Initialize
       force = forcing_imXY(i,j);
       [~, forceNum] = size(indices);
	   
	   %Over all nodes in 'stencil'
       for z = 1:forceNum
           delta = indices(:, z);
		   
		   %Here (and other places), 'root' refers to the center (i,j) stencil, and 'position' refers to
		   %the other stencil (i.e., (i+1, j) etc.).
           position = BoundaryAccess(position_imXYprev, i + delta(1), j + delta(2), settings, timeindex);
           root = position_imXYprev(i,j);
           rootVelocity = velocity_imXYprev(i,j);
           positionVelocity = vBoundaryAccess(velocity_imXYprev, i + delta(1), j + delta(2), settings, timeindex);
		   
		   %Compute contributing spring/spring damping force
           deltaForce = GetForce(root, position, k, req);
           deltaSpringDamp=DampForce(root, rootVelocity, position, positionVelocity, settings);
           deltaForce=deltaForce+deltaSpringDamp;
		   
		   %Check if on boundaries and if the wall binding condition is satisfied for that condition.
           if (i + delta(1) == 0) && ~settings.bindWall(1)%up
               deltaForce = 0;
           end
           if (i + delta(1) == (settings.Ny + 1)) && ~settings.bindWall(2)%down
               deltaForce = 0;
           end
           if (j + delta(2) == 0) && ~settings.bindWall(3)%left
               deltaForce = 0;
           end
           if (j + delta(2) == (settings.Nx + 1)) && ~settings.bindWall(4)%right
               deltaForce = 0;
           end
		   
		   %Increment force
           force = force + deltaForce;
       end
	   %'fluid immersion' damping force.
       damping = -velocity_imXYprev(i,j)*b;
	   
	   %from newton's law
       acceleration = (force + damping)/settings.massDistribution(i,j);
	   
	   %first-order approximation.
       next_imXYout(i,j) = position_imXYprev(i,j) + velocity_imXYprev(i,j)*timestep + acceleration*timestep^2;
       next_velocity_imXY_out(i,j) = velocity_imXYprev(i,j) + timestep*acceleration;
   end
end
end

