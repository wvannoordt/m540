function [avivisualizer] = visualize(timepoints, filename, settings)

%   Inputs: timepoints-> A (m,n,t) array of complex points representing x,y
%           locations of nodes
%
%   Outputs: avivisualizer-> Movie of frames 
%
%

%Compute axis bounds
left = min(min(min(real(timepoints(:,:,:)))));
right = max(max(max(real(timepoints(:,:,:)))));
left = min(left, min(real(settings.wallDisplacementProfile(3, :))));
right = max(right, 1 + max(real(settings.wallDisplacementProfile(4, :))));

down = min(min(min(imag(timepoints(:,:,:)))));
up = max(max(max(imag(timepoints(:,:,:)))));
down = min(down, min(imag(settings.wallDisplacementProfile(2, :))));
up = max(up, 1 + max(imag(settings.wallDisplacementProfile(1, :))));


%Inflate axis limits a little bit for a nice picture
deltax = abs(right - left);
deltay = abs(up - down);
meanx = 0.5*(right + left);
meany = 0.5*(up + down); %don't be a meany
left = meanx - 1.1*0.5*deltax;
right = meanx + 1.1*0.5*deltax;
up = meany + 1.1*0.5*deltay;
down = meany - 1.1*0.5*deltay;


[m, n, t] = size(timepoints);

%Setup for color rendering
eqRy = 1 / (m + 1);
eqRx = 1 / (n + 1);
geom_radius = settings.colorMapRatio;

axis tight manual 
set(gca,'nextplot','replacechildren'); %Makes it so when you plot it replaces before
v = VideoWriter(filename);
v.FrameRate = settings.outputFrameRate;
open(v);

line_width = settings.lineWidth;

for i=1:t
    thesepoints = timepoints(:,:,i);
    mat=reshape(timepoints(:,:,i),[m*n,1]);   %Reshapes matrix to vector in order to scatter
    %extra frame stuff goes here
    hold on
	
	%Over domain
    for ii = 1:m
        for jj = 1:n
		
		%Plot down-and-right elements if bindWall condition holds
            a = BoundaryAccess(thesepoints, ii, jj, settings, i);
            b = BoundaryAccess(thesepoints, ii+1, jj, settings, i);%down
            c = BoundaryAccess(thesepoints, ii, jj+1, settings, i);%right
            x = [real(b) real(a)];
            y = [imag(b) imag(a)];
            on_end_down = (ii + 1 == settings.Ny + 1);
            if ~(~settings.bindWall(2) && on_end_down)%down
            plot(x, y, 'color', RedBlueMap((1 - geom_radius)*eqRy,(1 + geom_radius)*eqRy,abs(b - a)), 'LineWidth', line_width);
            end
            hold on;
            xx = [real(c) real(a)];
            yy = [imag(c) imag(a)];
            on_end_right = (jj + 1 == settings.Nx + 1);
            if ~(~settings.bindWall(4) && on_end_right)%right
            plot(xx, yy, 'color', RedBlueMap((1 - geom_radius)*eqRx,(1 + geom_radius)*eqRx,abs(c - a)), 'LineWidth', line_width);
            end
        end
    end
    if settings.bindWall(3)%left
    for ii = 1:m
        a = BoundaryAccess(thesepoints, ii, 1, settings, i);
        b = BoundaryAccess(thesepoints, ii, 0, settings, i);
        x = [real(a) real(b)];
        y = [imag(a) imag(b)];
        plot(x, y, 'color', RedBlueMap((1 - geom_radius)*eqRx,(1 + geom_radius)*eqRx,abs(b - a)), 'LineWidth', line_width);
        hold on;
    end
    end
    if settings.bindWall(1)%up
    for jj= 1:n
        a = BoundaryAccess(thesepoints, 0, jj, settings, i);
        b = BoundaryAccess(thesepoints, 1, jj, settings, i);
        x = [real(a) real(b)];
        y = [imag(a) imag(b)];
        plot(x, y, 'color', RedBlueMap((1 - geom_radius)*eqRy,(1 + geom_radius)*eqRy,abs(b - a)), 'LineWidth', line_width);
        hold on;
    end
    end
    %Plot the boundaries
    %left wall
    wl_a = 0 + settings.wallDisplacementProfile(3, i);
    wl_b = 1i + settings.wallDisplacementProfile(3, i);
    if settings.bindWall(3)
        plot(real([wl_a wl_b]), imag([wl_a wl_b]), 'color', [0 0 0], 'LineWidth', settings.wallLineWidth);
    end
    
    %lower wall
    wl_a = 0 + settings.wallDisplacementProfile(2, i);
    wl_b = 1 + settings.wallDisplacementProfile(2, i);
    if settings.bindWall(2)
        plot(real([wl_a wl_b]), imag([wl_a wl_b]), 'color', [0 0 0], 'LineWidth', settings.wallLineWidth);
    end
    
    %right wall
    wl_a = 1+1i + settings.wallDisplacementProfile(4, i);
    wl_b = 1 + settings.wallDisplacementProfile(4, i);
    if settings.bindWall(4)
        plot(real([wl_a wl_b]), imag([wl_a wl_b]), 'color', [0 0 0], 'LineWidth', settings.wallLineWidth);
    end
    
    %upper wall
    wl_a = 1i + settings.wallDisplacementProfile(1, i);
    wl_b = 1+1i + settings.wallDisplacementProfile(1, i);
    if settings.bindWall(1)
        plot(real([wl_a wl_b]), imag([wl_a wl_b]), 'color', [0 0 0], 'LineWidth', settings.wallLineWidth);
    end
    axis([left right down up]);
    time = (i - 1) * settings.timeStep;
    title(strcat('t = ', num2str(time)));
    %end extra frame stuff
    frame = getframe(gcf);                    %Grabs the frame
    writeVideo(v,frame)   
    clf
end
close(v);
 
  
end
