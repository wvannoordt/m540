function [color] = RedBlueMap(low, high, value)
%This function proportionally maps 'value' between 'low' and 'high', outputs
%the corresponding color between red and blue (linear homotopy)
blue = [0 0 1];
red = [1 0 0];
if value <= low
    color = blue;
elseif value >= high
        color = red;
else
    theta = (value - low)/(high - low);
    color = (1-theta)*blue + theta * red;
end
end

