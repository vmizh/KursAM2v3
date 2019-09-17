/// <binding ProjectOpened='default' />
var gulp = require('gulp');
var sass = require('gulp-sass');
var rimraf = require("rimraf"),
    concat = require("gulp-concat"),
    cssmin = require("gulp-cssmin"),
    uglify = require("gulp-uglify");

var paths = {
    webroot: "./KURS2AMC/"
};

paths.sass = paths.webroot + "scss/**/*.scss";
paths.js = paths.webroot + "Content/js/**/*.js";
paths.minJs = paths.webroot + "Content/js/**/*.min.js";
paths.css = paths.webroot + "Content/css/**/*.css";
paths.minCss = paths.webroot + "Content/css/**/*.min.css";
paths.concatJsDest = paths.webroot + "Content/js/site.min.js";
paths.concatCssDest = paths.webroot + "Content/css/site.min.css";

gulp.task('styles', function () {
    gulp.src('scss/**/*.scss')
        .pipe(sass().on('error', sass.logError))
        .pipe(gulp.dest('./Content/css/'))
});

//Watch task
gulp.task('default', function () {
    gulp.watch('scss/**/*.scss', ['styles']);
});