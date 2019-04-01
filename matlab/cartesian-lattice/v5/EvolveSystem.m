function [system_out] = EvolveSystem(settings)
%This function serves as an iterator for the system defined in the settings struct.

%Initialize the system
Nx = settings.Nx;
Ny = settings.Ny;
Nt = settings.Nt;
x = (1:Nx)/(Nx+1);
y = (1:Ny)/(Ny+1);

%Distibrute nodes uniformly on a square domain.
[X, Y] = meshgrid(x,y);
initial_XY = X + 1i*flipud(Y);

%Initialize some stuff
system_out = zeros(Ny, Nx, Nt);
system_out(:,:,1) = initial_XY;
previous_velocity = zeros(Ny, Nx);

%Euler's method basically. If you want to implement a higher-order method I'll give u a high five.
for k = 2:Nt
   [xs, vs] = IterateCartesianLattice(system_out(:,:,k-1), previous_velocity, settings.forceTensor(:,:,k), settings, k);
   previous_velocity = vs;
   system_out(:,:,k) = xs;
end
end

