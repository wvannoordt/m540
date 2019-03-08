function avivisualizer= visualize(timepoints)

%   Inputs: timepoints-> A (m,n,t) array of complex points representing x,y
%           locations of nodes
%
%   Outputs: avivisualizer-> Movie of frames 
%
%

m=size(timepoints,1); n=size(timepoints,2); t=size(timepoints,3);


axis tight manual 
set(gca,'nextplot','replacechildren'); %Makes it so when you plot it replaces before
v = VideoWriter('crystal.avi');
v.FrameRate = 5;
open(v);

for i=1:t
    mat=reshape(timepoints(:,:,i),[m*n,1]);   %Reshapes matrix to vector in order to scatter
    scatter(real(mat),imag(mat))              %Scatters real and imaginary part (X and Y terms)
    frame = getframe(gcf);                    %Grabs the frame
    writeVideo(v,frame)     
end

close(v);
 
  
end
