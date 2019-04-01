clear
clc
close all

tic

%Admin stuff, not really important.
listing = dir('./output/');
vidnum = size(listing, 1) - 2;

%Set up the settings' numerical values

%Simulation size
simSettings.Nx = 20;
simSettings.Ny = 20;
simSettings.Nt = 200;

%Hooke's law spring constant
simSettings.springConstant = 60;

%Self explanitory
simSettings.equilibriumLength = 0.03;
simSettings.timeStep = 0.1;
simSettings.dampingCoefficient = 1;

%There is a distinction between damping coefficients here - see DampForce.m
simSettings.springDampingCoefficient = 0;

%Initialize mass of each node in domain
simSettings.massDistribution = ones(simSettings.Ny, simSettings.Nx);

%Self-explanitory
simSettings.outputFrameRate = 60;

%Initialize forcing term for each node, for each timestep.
simSettings.forceTensor = zeros(simSettings.Ny, simSettings.Nx, simSettings.Nt);

%All profiles are expressed in terms of displacement from initial position, forming a unit square.
simSettings.wallDisplacementProfile = zeros(4, simSettings.Nt); %up down right left %Let (4,:)=f(t) to set function on upper wall

%Graphical thing
simSettings.colorMapRatio = 0.3;

%Sets whether or not each wall has springs bound to it.
simSettings.bindWall = [true true true true]; %up down left right

%Graphical settings
simSettings.lineWidth = 2;
simSettings.wallLineWidth = 3;


for i = 1:simSettings.Nt
   simSettings.forceTensor(1, 1, i) = 5*exp(-0.13*i*1i*pi)*exp(-0.02*i);
end

%Run simulation
data = EvolveSystem(simSettings);
runtime = toc
tic

%IO stuff
filename = strcat('output/crystal', num2str(vidnum), '.avi');
while exist(filename, 'file') == 2
    vidnum = vidnum + 1;
    filename = strcat('output/crystal', num2str(vidnum), '.avi');
end

%Output video.
visualize(data, filename, simSettings);
visualizetime = toc

%This line prints 'Done.' to the console. After the simulation runs, it is critical that the user knows that it is 
%done running, and this line does that.
fprintf('Done.');

close all