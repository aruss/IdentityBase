var gulp = require('gulp');
var sass = require('gulp-sass');
var concat = require('gulp-concat');
var browserSync = require('browser-sync');
var uglify = require('gulp-uglify');
var sourcemaps = require('gulp-sourcemaps');

gulp.task('styles', function () {

    return gulp.src('./wwwroot/css/**/*.scss')
        .pipe(sourcemaps.init())
        .pipe(concat('theme.min.css'))
        .pipe(sass({ outputStyle: 'compressed' }).on('error', sass.logError))
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest('./wwwroot/css/'));
});

gulp.task('scripts', function() {
    return gulp.src([
        './node_modules/jquery/dist/jquery.js',
        './node_modules/jquery-validation/dist/jquery.validate.js',
        './node_modules/jquery-validation-unobtrusive/dist/jquery.validate.unobtrusive.js',
        './node_modules/bootstrap/dist/js/bootstrap.js',
        './wwwroot/js/theme.js'
    ])
        .pipe(sourcemaps.init())
        .pipe(concat('theme.min.js'))
        .pipe(uglify())
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest('./wwwroot/js/'));
});

gulp.task('lib', function () {

    // copy here different required junk from node_modules folder

});

gulp.task('build', ['lib', 'styles', 'scripts']);

gulp.task('serve', ['build'], function () {

    browserSync.init(null, {
        proxy: "http://localhost:5000",
    });

    watchStyles();
    watchScripts();

    gulp.watch([
        './Views/**/*.cshtml',
        './wwwroot/js/**/*.min.js',
        './wwwroot/css/**/*.min.css'
    ]).on('change', browserSync.reload);
});

gulp.task('watch', ['build'], function () {
    watchStyles();
    watchScripts();
});

function watchStyles() {
    gulp.watch([
        './wwwroot/css/**/*.scss'
    ], ['styles']);
}

function watchScripts() {
    gulp.watch([
        './wwwroot/js/**/*.js'
    ], ['scripts']);
}