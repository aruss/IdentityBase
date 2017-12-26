var gulp = require('gulp'),
    sass = require('gulp-sass'),
    concat = require('gulp-concat');
browserSync = require('browser-sync');

gulp.task('watch', ['build'], function () {

    return gulp.watch([
        './Public/css/**/*.scss',
        './Public/css/**/_*.scss'
    ], ['styles'])
});

gulp.task('styles', function () {

    return gulp.src('./Public/css/**/*.scss')
        .pipe(sass({ outputStyle: 'compressed' }).on('error', sass.logError))
        .pipe(gulp.dest('./Public/css/'));
});

gulp.task('lib', function () {

    // jQuery
    // gulp.src('./node_modules/jquery/dist/jquery.min.js')
    //   .pipe(gulp.dest('./Public/js'));
});

gulp.task('build', ['lib', 'styles']);

gulp.task('serve', ['build'], function () {

    browserSync.init(null, {
        proxy: "http://localhost:5000",
    });

    gulp.watch([
        './Views/**/*.cshtml',
        './Public/js/**/*.js',
        './Public/css/**/*.css'
    ]).on('change', browserSync.reload);

    gulp.watch([
        './Public/css/**/*.scss',
        './Public/css/**/_*.scss'
    ], ['styles']);
});

