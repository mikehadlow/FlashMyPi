/// <reference path="d3.d.ts" />

window.onload = () => {
    var size = 8
    var square = 50;
    var border = 2;

    var svg = d3.select("#svg")
        .attr("width", square * size)
        .attr("height", square * size);

    for (var i = 0; i < size; i++) {
        for (var j = 0; j < size; j++) {
            var rect = svg.append("rect")
                .attr("height", square - border)
                .attr("width", square - border)
                .attr("x", square * i)
                .attr("y", square * j);
        }
    }
};