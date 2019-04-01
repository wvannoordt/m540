function [complexForce] = GetForce(rootpos, springpos, k, req)
%Hooke's law, that's all.

length = abs(springpos - rootpos);
direction = (springpos - rootpos) / length;
magnitude = k * (length - req);
complexForce = magnitude*direction;
end

