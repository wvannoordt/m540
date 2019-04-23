clear
clc
close all

data_d = csvread('sphere_with_degeneracy_detection.csv');
data_nd = csvread('sphere_without_degeneracy_detection.csv');
subplot(1,2,1);
pcshow(data_d)
title('Degeneracy detection')
subplot(1,2,2);
pcshow(data_nd)
title('No degeneracy detection')
